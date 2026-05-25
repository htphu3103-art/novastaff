import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserRole } from '../modules/auth/types';

const ProtectedRoute = ({ allowedRoles }: { allowedRoles: string[] }) => {
    const { user, isAuthenticated } = useAuth();

    if (!isAuthenticated || !user) return <Navigate to="/login" replace />;

    // Chuyển đổi allowedRoles thành numeric roles để so sánh
    const isAllowed = allowedRoles.some(role => {
        const target = role.toLowerCase();
        if (target === 'admin') return user.role === UserRole.Admin;
        if (target === 'manager') return user.role === UserRole.Manager;
        if (target === 'staff') return user.role === UserRole.Staff;
        
        // Hoặc so khớp trực tiếp nếu pass ID dưới dạng string '1', '2'...
        return target === user.role.toString();
    });

    if (!isAllowed) {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
};
export default ProtectedRoute;