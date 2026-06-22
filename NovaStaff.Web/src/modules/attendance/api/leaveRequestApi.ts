import { axiosClient as api } from "../../../utils/axiosClient";
import {
    LeaveRequestDto,
    CreateLeaveRequest,
    RejectLeaveRequest
} from "../types";

const BASE_URL = "leave-requests";

export const leaveRequestApi = {
    // =========================================================
    // READ
    // =========================================================

    /** Lấy danh sách đơn nghỉ của bản thân (current user) */
    getMyRequests: () =>
        api.get<LeaveRequestDto[]>(`${BASE_URL}/me`),

    /** Lấy danh sách đơn nghỉ theo employeeId */
    getByEmployee: (employeeId: number) =>
        api.get<LeaveRequestDto[]>(`${BASE_URL}/employee/${employeeId}`),

    /** Lấy danh sách đơn đang Pending (HR/Manager dùng) */
    getPending: (departmentId?: number) =>
        api.get<LeaveRequestDto[]>(`${BASE_URL}/pending`, {
            params: { departmentId }
        }),

    /** Tổng số ngày nghỉ đã được duyệt trong năm của nhân viên */
    getApprovedDays: (employeeId: number, year: number) =>
        api.get<{ employeeId: number; year: number; approvedDays: number }>(
            `${BASE_URL}/employee/${employeeId}/approved-days`,
            { params: { year } }
        ),

    // =========================================================
    // CREATE
    // =========================================================

    /** Tạo đơn xin nghỉ mới */
    create: (data: CreateLeaveRequest) =>
        api.post<LeaveRequestDto>(BASE_URL, data),

    // =========================================================
    // APPROVAL FLOW
    // =========================================================

    /** Duyệt đơn nghỉ */
    approve: (requestId: number) =>
        api.post(`${BASE_URL}/${requestId}/approve`),

    /** Từ chối đơn nghỉ */
    reject: (requestId: number, data?: RejectLeaveRequest) =>
        api.post(`${BASE_URL}/${requestId}/reject`, data),
};
