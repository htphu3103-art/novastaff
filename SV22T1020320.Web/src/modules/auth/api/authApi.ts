import { axiosClient as api } from "../../../utils/axiosClient";
import {
    AuthResponse,
    LoginRequest,
    CreateUserRequest,
    ChangePasswordRequest,
    UpdateUserRoleRequest,
    UserProfileDto,
    UserRole,
    ActivateAccountRequest
} from "../types";

export const authApi = {
    // ============================================================
    // 🔐 NHÓM XÁC THỰC (AUTHENTICATION)
    // ============================================================

    login: (data: LoginRequest) =>
        api.post<AuthResponse>("/auth/login", data),

    logout: () =>
        api.post("/auth/logout"),

    refreshToken: () =>
        api.post<{ accessToken: string }>("/auth/refresh"),

    activateAccount: (data: ActivateAccountRequest) =>
        api.post("/auth/activate", data),

    // ============================================================
    // 👤 NHÓM NGƯỜI DÙNG - CÁ NHÂN (PROFILE / SELF)
    // ============================================================

    /** Lấy profile cá nhân hiện tại */
    getCurrentUser: (signal?: AbortSignal) =>
        api.get<UserProfileDto>("/users/me", { signal }),

    /** Đổi mật khẩu cá nhân */
    changePassword: (data: ChangePasswordRequest) =>
        api.post("/users/change-password", data),


    // ============================================================
    // 🛠️ NHÓM QUẢN TRỊ (ADMIN ONLY)
    // ============================================================

    /** Tạo tài khoản mới (Admin) */
    create: (data: CreateUserRequest) =>
        api.post("/users", data),

    /** Lấy thông tin user theo ID (Admin) */
    getById: (id: number) =>
        api.get(`/users/${id}`),

    /** Cập nhật quyền (Role) cho user (Admin) */
    updateRole: (id: number, role: UserRole) =>
        api.put(`/users/${id}/role`, { role }),

    /** Khóa tài khoản (Admin) */
    lock: (id: number) =>
        api.put(`/users/${id}/lock`),

    /** Mở khóa tài khoản (Admin) */
    unlock: (id: number) =>
        api.put(`/users/${id}/unlock`),

    /** Cấp lại mật khẩu (Admin reset password) */
    resetPassword: (id: number) =>
        api.post<{ password: string }>(`/users/${id}/reset-password`),
};