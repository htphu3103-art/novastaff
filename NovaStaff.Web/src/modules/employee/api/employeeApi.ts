// api/employeeApi.ts
import { axiosClient as api } from "../../../utils/axiosClient";
import { EmployeeStatus } from "../types";
import {
    EmployeeDto,
    PagedResult,
    EmployeeFilter,
    CreateEmployeeRequest,
    UpdateEmployeeRequest,
    TransferDepartmentRequest,
    EmployeeManagerDto,
} from "../types";

export const employeeApi = {
    // ============================================================
    // 📖 NHÓM TRUY VẤN (READ)
    // ============================================================

    /**
     * Lấy danh sách nhân viên phân trang có lọc.
     * Gọi đến: [HttpGet] api/employees
     */
    getPaged: (filter: EmployeeFilter, pageIndex = 1, pageSize = 20, signal?: AbortSignal) =>
        api.get<PagedResult<EmployeeDto>>("/employees", {
            signal,
            params: {
                ...filter,
                pageIndex,
                pageSize,
            },
        }),

    /**
     * Lấy chi tiết thông tin một nhân viên theo ID.
     * Gọi đến: [HttpGet("{id:int}")] api/employees/5
     */
    getById: (id: number, signal?: AbortSignal) =>
        api.get<EmployeeDto>(`/employees/${id}`, { signal }),

    /**
     * Lấy nhân viên theo mã nhân viên (Code).
     * Gọi đến: [HttpGet("code/{code}")] api/employees/code/NV1001
     */
    getByCode: (code: string, signal?: AbortSignal) =>
        api.get<EmployeeDto>(`/employees/code/${code}`, { signal }),

    /**
     * Lấy danh sách nhân viên thuộc một phòng ban cụ thể.
     * Gọi đến: [HttpGet("department/{departmentId:int}")] api/employees/department/3
     */
    getByDepartment: (departmentId: number, signal?: AbortSignal) =>
        api.get<EmployeeDto[]>(`/employees/department/${departmentId}`, { signal }),

    /**
     * Lấy danh sách cấp dưới của một nhân viên.
     * Gọi đến: [HttpGet("{id:int}/subordinates")] api/employees/5/subordinates
     */
    getSubordinates: (id: number, signal?: AbortSignal) =>
        api.get<EmployeeDto[]>(`/employees/${id}/subordinates`, { signal }),

    /**
     * Lấy danh sách nhân viên quản lý.
     * Gọi đến: [HttpGet("managers")] api/employees/managers
     */
    getManagers: (signal?: AbortSignal) =>
        api.get<EmployeeManagerDto[]>("/employees/managers", { signal }),



    // ============================================================
    // ✍️ NHÓM THAY ĐỔI DỮ LIỆU (WRITE)
    // ============================================================

    /**
     * Tạo mới nhân viên.
     * Gọi đến: [HttpPost] api/employees
     */
    create: (data: CreateEmployeeRequest) =>
        api.post<EmployeeDto>("/employees", data),

    /**
     * Cập nhật thông tin nhân viên.
     * Gọi đến: [HttpPut("{id:int}")] api/employees/5
     */
    update: (id: number, data: UpdateEmployeeRequest) =>
        api.put<EmployeeDto>(`/employees/${id}`, data),

    /**
     * Xóa nhân viên.
     * Gọi đến: [HttpDelete("{id:int}")] api/employees/5
     */
    delete: (id: number) =>
        api.delete(`/employees/${id}`),

    /**
     * Điều chuyển phòng ban cho nhân viên.
     * Gọi đến: [HttpPut("{id:int}/transfer")] api/employees/5/transfer
     */
    transfer: (id: number, request: TransferDepartmentRequest) =>
        api.put(`/employees/${id}/transfer`, request),

    changeStatus: (
        id: number,
        status: EmployeeStatus
    ) =>
        api.put(`/employees/${id}/status`, {
            status,
        }),
};