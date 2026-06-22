import React, { useEffect, useState } from "react";
import { Spin } from "antd";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate, useLocation } from "react-router-dom";

export const GlobalAuthInit = ({ children }: { children: React.ReactNode }) => {
    const { hydrate, logout } = useAuth();
    const [isHydrated, setIsHydrated] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        let isMounted = true;

        const initAuth = async () => {
            await hydrate();
            if (isMounted) {
                setIsHydrated(true);
            }
        };

        initAuth();

        return () => {
            isMounted = false;
        };
    }, [hydrate]);

    useEffect(() => {
        // Lắng nghe sự kiện logout từ axiosClient khi gặp 401
        const handleAuthLogout = () => {
            logout();
            // Các public routes không bị ép về trang đăng nhập
            const publicRoutes = ["/login", "/activate"];
            if (!publicRoutes.includes(location.pathname)) {
                navigate("/login");
            }
        };

        window.addEventListener("auth:logout", handleAuthLogout);

        return () => {
            window.removeEventListener("auth:logout", handleAuthLogout);
        };
    }, [logout, navigate, location.pathname]);

    if (!isHydrated) {
        return (
            <div style={{ height: "100vh", display: "flex", justifyContent: "center", alignItems: "center", background: "#f0f2f5" }}>
                <Spin size="large" description="Đang kiểm tra phiên đăng nhập..." />
            </div>
        );
    }

    return <>{children}</>;
};
