import { axiosClient as api } from "../../../utils/axiosClient";
import {
    WorkTaskDto,
    PagedResult,
    WorkTaskFilter,
    CreateWorkTaskRequest,
    UpdateWorkTaskRequest,
    ChangeTaskStatusRequest,
    WorkTaskStatisticsDto,
} from "../types";

export const taskApi = {
    /**
     * Lấy chi tiết công việc theo ID.
     * Gọi đến: [HttpGet("{id}")] api/work-tasks/123
     */
    getById: (id: number, signal?: AbortSignal) =>
        api.get<WorkTaskDto>(`/work-tasks/${id}`, { signal }),

    /**
     * Lấy danh sách công việc phân trang có lọc.
     * Gọi đến: [HttpGet] api/work-tasks
     */
    getPaged: (filter: WorkTaskFilter, pageIndex = 1, pageSize = 10, signal?: AbortSignal) =>
        api.get<PagedResult<WorkTaskDto>>("/work-tasks", {
            signal,
            params: {
                ...filter,
                pageIndex,
                pageSize,
            },
        }),

    /**
     * Lấy danh sách công việc theo người được giao (Assignee).
     * Gọi đến: [HttpGet("assignee/{employeeId}")] api/work-tasks/assignee/5
     */
    getByAssignee: (employeeId: number, pageIndex = 1, pageSize = 10, signal?: AbortSignal) =>
        api.get<PagedResult<WorkTaskDto>>(`/work-tasks/assignee/${employeeId}`, {
            signal,
            params: { pageIndex, pageSize },
        }),

    /**
     * Lấy danh sách công việc quá hạn.
     * Gọi đến: [HttpGet("overdue")] api/work-tasks/overdue
     */
    getOverdueTasks: (pageIndex = 1, pageSize = 10, signal?: AbortSignal) =>
        api.get<PagedResult<WorkTaskDto>>("/work-tasks/overdue", {
            signal,
            params: { pageIndex, pageSize },
        }),

    /**
     * Lấy danh sách công việc theo người quản lý.
     * Gọi đến: [HttpGet("manager/{managerId}")] api/work-tasks/manager/3
     */
    getByManager: (managerId: number, pageIndex = 1, pageSize = 10, signal?: AbortSignal) =>
        api.get<PagedResult<WorkTaskDto>>(`/work-tasks/manager/${managerId}`, {
            signal,
            params: { pageIndex, pageSize },
        }),

    /**
     * Lấy thống kê trạng thái công việc trong khoảng thời gian.
     * Gọi đến: [HttpGet("statistics")] api/work-tasks/statistics
     */
    getStatistics: (startDate: string, endDate: string, signal?: AbortSignal) =>
        api.get<WorkTaskStatisticsDto[]>("/work-tasks/statistics", {
            signal,
            params: { startDate, endDate },
        }),

    /**
     * Tạo mới một công việc.
     * Gọi đến: [HttpPost] api/work-tasks
     */
    create: (data: CreateWorkTaskRequest) =>
        api.post<WorkTaskDto>("/work-tasks", data),

    /**
     * Cập nhật toàn bộ thông tin công việc.
     * Gọi đến: [HttpPut("{id}")] api/work-tasks/123
     */
    update: (id: number, data: UpdateWorkTaskRequest) =>
        api.put<WorkTaskDto>(`/work-tasks/${id}`, data),

    /**
     * Xóa một công việc.
     * Gọi đến: [HttpDelete("{id}")] api/work-tasks/123
     */
    delete: (id: number) =>
        api.delete(`/work-tasks/${id}`),

    /**
     * Chỉ cập nhật trạng thái của công việc.
     * Gọi đến: [HttpPatch("{id}/status")] api/work-tasks/123/status
     */
    changeStatus: (id: number, data: ChangeTaskStatusRequest) =>
        api.patch(`/work-tasks/${id}/status`, data),

    /**
     * Xác nhận công việc thật sự hoàn thành.
     * Gọi đến: [HttpPatch("{id}/complete")] api/work-tasks/123/complete
     */
    complete: (id: number) =>
        api.patch(`/work-tasks/${id}/complete`),
};
