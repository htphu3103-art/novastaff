// ============================================================
// 📁 ENUMS
// ============================================================

export enum AttendanceStatus {
    Unknown = 0,
    Present = 1,
    Late = 2,
    Absent = 3,
    HalfDay = 4,
    Leave = 5
}

export enum LeaveType {
    Annual = 0,
    Sick = 1,
    Unpaid = 2,
    Maternity = 3,
    Other = 4
}

export enum LeaveRequestStatus {
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

export enum AttendanceSortField {
    WorkDate = 0,
    EmployeeCode = 1,
    WorkHours = 2,
    Status = 3
}

// ============================================================
// 📄 DTOs
// ============================================================

export interface AttendanceDto {
    recordId: number;
    employeeId: number | null;
    employeeCode: string | null;
    employeeName: string | null;

    workDate: string; // ISO Date
    checkIn: string | null; // ISO DateTime
    checkOut: string | null; // ISO DateTime
    workHours: number | null;
    status: AttendanceStatus;
    statusDisplay: string;
    note: string | null;

    isCheckedIn: boolean;
    isCheckedOut: boolean;

    // Audit fields
    createdBy: number | null;
    createdByName: string | null;
    createdDate: string;
    modifiedBy: number | null;
    modifiedByName: string | null;
    modifiedDate: string | null;
}

export interface LeaveRequestDto {
    requestId: number;
    employeeId: number | null;
    employeeCode: string | null;
    employeeName: string | null;
    leaveType: LeaveType;
    fromDate: string; // ISO Date
    toDate: string; // ISO Date
    totalDays: number;
    isHalfDayStart: boolean;
    isHalfDayEnd: boolean;
    reason: string | null;
    status: LeaveRequestStatus;
    approvedBy: number | null;
    approvedDate: string | null;
    createdDate: string; // ISO Date
}

// ============================================================
// 📥 REQUESTS
// ============================================================

export interface CreateLeaveRequest {
    employeeId: number;
    leaveType: LeaveType;
    fromDate: string;
    toDate: string;
    isHalfDayStart: boolean;
    isHalfDayEnd: boolean;
    reason?: string | null;
}

export interface UpdateLeaveRequest {
    leaveType: LeaveType;
    fromDate: string;
    toDate: string;
    isHalfDayStart: boolean;
    isHalfDayEnd: boolean;
    reason?: string | null;
}

export interface ApproveLeaveRequest {
    isApproved: boolean;
    note?: string | null;
}

export interface RejectLeaveRequest {
    reason?: string | null;
}

export interface LeaveRequestFilter {
    employeeId?: number;
    departmentId?: number;
    status?: LeaveRequestStatus;
    leaveType?: LeaveType;
    from?: string; // ISO Date
    to?: string; // ISO Date
    sortDescending: boolean;
}

export interface CreateAttendanceRequest {
    employeeId: number;
    workDate: string;
    checkIn?: string | null;
    checkOut?: string | null;
    status: AttendanceStatus;
    note?: string | null;
}

export interface UpdateAttendanceRequest {
    checkIn?: string | null;
    checkOut?: string | null;
    status: AttendanceStatus;
    note?: string | null;
}

export interface AttendanceFilter {
    employeeId?: number;
    departmentId?: number;
    from?: string; // ISO Date
    to?: string; // ISO Date
    year?: number;
    month?: number;
    status?: AttendanceStatus;
    sortBy?: AttendanceSortField;
    sortDescending?: boolean;
}

// ============================================================
// 🔍 PAGINATION
// ============================================================

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}
