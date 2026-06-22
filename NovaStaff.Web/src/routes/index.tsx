import { Routes, Route, Navigate } from "react-router-dom"
import MainLayout from "../layouts/MainLayout/MainLayout"
import DashboardPage from "../modules/dashboard/DashboardPage";
// CHÚ Ý: Nếu file DepartmentPage dùng "export const", hãy thêm { } như dòng dưới:
import DepartmentPage from "../modules/departments/DepartmentPage"
import TaskPage from "../modules/task/TaskPage"
import ChatPage from "../modules/chat/ChatPage"
import AttendancePage from "../modules/attendance/AttendancePage"
import PayrollPage from "../modules/payroll/PayrollPage"
import LoginPage from "../modules/auth/LoginPage"
import ActivateAccountPage from "../modules/auth/ActivateAccountPage"
import ProtectedRoute from "./ProtectedRoute"

export default function AppRoutes() {
    return (
        <Routes>
            {/* PUBLIC ROUTES */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/activate" element={<ActivateAccountPage />} />

            {/* PRIVATE ROUTES (Yêu cầu đăng nhập) */}
            <Route element={<MainLayout />}>

                {/* 1. Nhóm dùng chung cho cả Admin & Employee */}
                <Route path="/" element={<DashboardPage />} />
                <Route path="/tasks" element={<TaskPage />} />
                <Route path="/attendance" element={<AttendancePage />} />
                <Route path="/payroll" element={<PayrollPage />} />
                <Route path="/chat" element={<ChatPage />} />

                {/* 2. Nhóm chỉ dành cho ADMIN & MANAGER */}
                <Route element={<ProtectedRoute allowedRoles={['Admin', 'Manager']} />}>
                    <Route path="/departments" element={<DepartmentPage />} />
                </Route>

            </Route>

            {/* 404 - Điều hướng về trang chủ */}
            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
    )
}