import axios, { AxiosInstance, InternalAxiosRequestConfig } from "axios";

interface AxiosRequestConfigWithRetry extends InternalAxiosRequestConfig {
    _retry?: boolean;
}

let isRefreshing = false;
let failedQueue: Array<{
    resolve: (token: string) => void;
    reject: (error: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) prom.reject(error);
        else prom.resolve(token!);
    });
    failedQueue = [];
};

export const axiosClient: AxiosInstance = axios.create({
    baseURL: "http://localhost:5102/api",
    headers: {
        "Content-Type": "application/json",
    },
    withCredentials: true, // Quan trọng để gửi RefreshToken Cookie
});

// Request Interceptor: Đính kèm token
axiosClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: Xử lý Refresh Token tự động
axiosClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as AxiosRequestConfigWithRetry;

        // Nếu API refresh bị lỗi hoặc 401 không phải do token hết hạn -> Logout
        if (originalRequest.url?.includes("/auth/refresh")) {
            processQueue(error, null);
            window.dispatchEvent(new Event("auth:logout"));
            return Promise.reject(error);
        }

        if (error.response?.status === 401) {
            if (originalRequest._retry) {
                window.dispatchEvent(new Event("auth:logout"));
                return Promise.reject(error);
            }

            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then((token) => {
                        originalRequest.headers.Authorization = `Bearer ${token}`;
                        originalRequest._retry = true; // Add this to prevent infinite loop on queued requests
                        return axiosClient(originalRequest);
                    })
                    .catch((err) => {
                        window.dispatchEvent(new Event("auth:logout"));
                        return Promise.reject(err);
                    });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                // Gọi API refresh (withCredentials: true sẽ tự gửi HttpOnly Cookie)
                const response = await axios.post<{ accessToken: string }>(
                    `${axiosClient.defaults.baseURL}/auth/refresh`,
                    {},
                    { withCredentials: true }
                );

                const { accessToken } = response.data;
                localStorage.setItem("token", accessToken);
                axiosClient.defaults.headers.common["Authorization"] = `Bearer ${accessToken}`;

                processQueue(null, accessToken);
                return axiosClient(originalRequest);
            } catch (refreshError) {
                processQueue(refreshError, null);
                window.dispatchEvent(new Event("auth:logout"));
                return Promise.reject(refreshError);
            } finally {
                isRefreshing = false;
            }
        }

        return Promise.reject(error);
    }
);
