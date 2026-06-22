import { axiosClient as api } from "../../../utils/axiosClient";
import {
    AttendanceDto,
    AttendanceFilter,
    CreateAttendanceRequest,
    UpdateAttendanceRequest,
    PagedResult
} from "../types";

export const BASE_URL = "attendance";
export const attendanceApi = {
    // =========================================================
    // READ
    // =========================================================

    /** Lấy record chấm công hôm nay của nhân viên */
    getToday: (employeeId: number) =>
        api.get<AttendanceDto>(`/Attendance/today/${employeeId}`),

    /** Lấy record chấm công hôm nay của chính mình (Current User) */
    getTodaySelf: () =>
        api.get<AttendanceDto>("/Attendance/today"),

    /** Lấy bảng công theo tháng của nhân viên */
    getByEmployeeAndMonth: (employeeId: number, year: number, month: number) =>
        api.get<AttendanceDto[]>(`/Attendance/employee/${employeeId}`, {
            params: { year, month }
        }),

    /** Tổng giờ làm trong tháng của nhân viên */
    getTotalHours: (employeeId: number, year: number, month: number) =>
        api.get<{ employeeId: number; year: number; month: number; totalHours: number }>(
            `/Attendance/employee/${employeeId}/total-hours`,
            { params: { year, month } }
        ),

    /** Tổng giờ làm trong tháng của chính mình (Current User) */
    getTotalHoursSelf: (year: number, month: number) =>
        api.get<{ year: number; month: number; totalHours: number }>(
            "/Attendance/me/total-hours",
            { params: { year, month } }
        ),

    /** HR: Danh sách chấm công có phân trang + filter */
    getPaged: (filter: AttendanceFilter, pageIndex = 1, pageSize = 20) =>
        api.get<PagedResult<AttendanceDto>>("/Attendance", {
            params: { ...filter, pageIndex, pageSize }
        }),

    /** Lấy chi tiết 1 bản ghi chấm công */
    getById: (recordId: number | string) =>
        api.get<AttendanceDto>(`/Attendance/${recordId}`),

    // =========================================================
    // ACTIONS — Check-in / Check-out
    // =========================================================

    /** Nhân viên check-in */
    checkIn: (employeeId: number) =>
        api.post<AttendanceDto>(`/Attendance/check-in/${employeeId}`),

    /** Chính mình check-in */
    checkInSelf: () =>
        api.post<AttendanceDto>("/Attendance/check-in"),

    /** Nhân viên check-out */
    checkOut: (employeeId: number) =>
        api.post<AttendanceDto>(`/Attendance/check-out/${employeeId}`),

    /** Chính mình check-out */
    checkOutSelf: () =>
        api.post<AttendanceDto>("/Attendance/check-out"),

    // =========================================================
    // HR MANAGEMENT
    // =========================================================

    /** HR tạo record thủ công */
    createManual: (data: CreateAttendanceRequest) =>
        api.post<AttendanceDto>("/Attendance", data),

    /** HR chỉnh sửa record */
    update: (recordId: number | string, data: UpdateAttendanceRequest) =>
        api.put<AttendanceDto>(`/Attendance/${recordId}`, data),

    /** HR xóa record */
    delete: (recordId: number | string) =>
        api.delete(`/Attendance/${recordId}`),
};
