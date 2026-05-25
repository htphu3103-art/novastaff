import { ReactNode } from "react";

// ============================================================
// 📁 ENUMS (Khớp chính xác với Backend)
// ============================================================

export enum WorkTaskStatus {
    Unknown = 0,
    Todo = 1,
    InProgress = 2,
    Done = 3
}

export enum WorkTaskPriority {
    Low = 1,
    Medium = 2,
    High = 3
}

// ============================================================
// 📄 DTOs (Data Transfer Objects)
// ============================================================

export interface WorkTaskDto {
    id: number;
    title: string;
    description: string | null;
    status: string;           // "Pending", "InProgress", "Completed", etc.
    priority: string;         // "Low", "Medium", "High"
    dueDate: string | null;   // ISO string
    createdDate: string;      // ISO string
    completedDate?: string | null; // Ngày chốt hoàn thành
    employeeId: number | null;
    assigneeName: string | null;
}

export interface WorkTaskStatisticsDto {
    status: string;
    count: number;
}

// ============================================================
// 📥 REQUESTS
// ============================================================

export interface CreateWorkTaskRequest {
    title: string;
    description?: string | null;
    priority: WorkTaskPriority;
    dueDate?: string | null;
    employeeId?: number | null;
}

export interface UpdateWorkTaskRequest {
    title: string;
    description?: string | null;
    priority: WorkTaskPriority;
    dueDate?: string | null;
    employeeId?: number | null;
    status?: WorkTaskStatus;
}

export interface ChangeTaskStatusRequest {
    status: WorkTaskStatus;
}

// ============================================================
// 🔍 FILTERS & PAGINATION
// ============================================================

export interface WorkTaskFilter {
    titleContains?: string;
    status?: WorkTaskStatus;
    priority?: WorkTaskPriority;
    employeeId?: number;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}

// ============================================================
// 🎨 UI TYPES (Keep for existing components)
// ============================================================

/**
 * @deprecated Dùng WorkTaskDto thay thế khi chuyển sang API thật
 */
export interface Task {
    id: string;
    title: string;
    description?: string; // Thêm Description cho UI
    status: 'todo' | 'inprogress' | 'done';
    priority: 'High' | 'Medium' | 'Low';
    assignee: string;
    department: string;
}

export interface ColumnType {
    label: string;
    key: Task['status'];
    color: string;
    icon?: ReactNode;
}

export const COLUMNS: ColumnType[] = [
    { label: "Chờ thực hiện", key: "todo", color: "#6366f1" },
    { label: "Đang thực hiện", key: "inprogress", color: "#f59e0b" },
    { label: "Đã hoàn thành", key: "done", color: "#10b981" }
];

export const STAFF_LIST: string[] = ["Nguyễn Văn A", "Trần Thị B", "Admin User"];

// DỮ LIỆU MẪU (MOCK DATA)
export const MOCK_TASKS: Task[] = [
    { id: "1", title: "Thiết kế Landing Page", description: "Thiết kế giao diện cho trang chủ mới của công ty.", status: "todo", priority: "High", assignee: "Nguyễn Văn A", department: "Marketing" },
    { id: "2", title: "Tích hợp API thanh toán", description: "Kết nối cổng thanh toán VNPay và Momo.", status: "inprogress", priority: "Medium", assignee: "Trần Thị B", department: "IT" },
    { id: "3", title: "Fix lỗi giao diện Mobile", description: "Lỗi hiển thị menu trên iPhone 13.", status: "todo", priority: "High", assignee: "Admin User", department: "IT" },
    { id: "4", title: "Viết Unit Test cho Module Task", description: "Đảm bảo coverage đạt 80%.", status: "done", priority: "Low", assignee: "Admin User", department: "IT" },
];

