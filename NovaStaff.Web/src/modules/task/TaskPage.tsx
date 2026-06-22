import React from "react";
import AdminTaskPage from './admin/AdminTaskPage';
import EmployeeTaskPage from './employee/EmployeeTaskPage';
import { useAuth } from "../../contexts/AuthContext";
import { UserRole } from "../auth/types";

export default function TaskPage() {
    const { user } = useAuth();

    if (!user) return null;

    // Nếu là admin (1) hoặc manager (2)
    if (user.role === UserRole.Admin || user.role === UserRole.Manager) {
        return <AdminTaskPage user={user} />;
    }

    return <EmployeeTaskPage user={user} />;
}