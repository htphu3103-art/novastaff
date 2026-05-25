import React, { createContext, useContext, useState, useCallback, ReactNode } from "react";
import { authApi } from "../modules/auth/api/authApi";
import { UserProfileDto, UserRole } from "../modules/auth/types";

interface AuthContextType {
    user: UserProfileDto | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (token: string, userData: UserProfileDto) => void;
    logout: () => void;
    hydrate: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const sanitizeUser = (userData: any): UserProfileDto => {
    if (!userData) return userData;
    const role = userData.role;
    
    // Nếu role là chuỗi (ví dụ "Admin"), chuyển sang số tương ứng
    if (typeof role === 'string') {
        const roleMap: Record<string, number> = {
            'Admin': UserRole.Admin,
            'Manager': UserRole.Manager,
            'Staff': UserRole.Staff
        };
        userData.role = roleMap[role] || UserRole.Staff;
    }
    return userData as UserProfileDto;
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<UserProfileDto | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const login = useCallback((token: string, userData: UserProfileDto) => {
        const cleanUser = sanitizeUser(userData);
        localStorage.setItem("token", token);
        localStorage.setItem("user", JSON.stringify(cleanUser));
        setUser(cleanUser);
    }, []);

    const logout = useCallback(async () => {
        const token = localStorage.getItem("token");
        
        // Chỉ gọi API logout nếu đang có token (để revoke trên server)
        if (token) {
            try {
                await authApi.logout();
            } catch (error) {
                // Log lỗi nhưng không chặn việc xóa local state
                console.error("Backend logout failed:", error);
            }
        }

        // Luôn luôn dọn dẹp local state bất kể API thành công hay thất bại
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        setUser(null);
    }, []);

    const hydrate = useCallback(async () => {
        const token = localStorage.getItem("token");
        if (!token) {
            logout();
            setIsLoading(false);
            return;
        }

        try {
            const res = await authApi.getCurrentUser();
            const userData = sanitizeUser(res.data);
            localStorage.setItem("user", JSON.stringify(userData));
            setUser(userData);
        } catch (error) {
            console.error("Hydration failed (Token expired or invalid)", error);
            logout();
        } finally {
            setIsLoading(false);
        }
    }, [logout]);

    return (
        <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout, hydrate }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
};
