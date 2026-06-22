    // api/departmentApi.ts
    import { axiosClient as api } from "../../../utils/axiosClient";
    import {
        DepartmentDto,
        PagedResult,
        CreateDepartmentRequest,
        UpdateDepartmentRequest,
    } from "../types";

    export const departmentApi = {
        // ============================================================
        // 📖 NHÓM TRUY VẤN (READ)
        // ============================================================

        /**
         * Lấy danh sách phòng ban gốc (Level 1).
         * Gọi đến: [HttpGet] GetAll trong Controller (service đang map về GetRootsAsync).
         *
         * Lưu ý: NameContains (nếu truyền) chỉ filter trong ROOTS, không phải search toàn cây.
         */
        getRootsPaged: (pageIndex = 1, pageSize = 10, nameContains?: string) =>
            api.get<PagedResult<DepartmentDto>>("/Departments", {
                params: {
                    PageIndex: pageIndex,
                    PageSize: pageSize,
                    NameContains: nameContains?.trim()
                },
            }),

        /**
         * PHẦN 2: Tìm kiếm / Lấy danh sách trong một nhánh cụ thể
         * Gọi đến: [HttpGet("{id:int}/descendants")] trong Controller
         * Sử dụng hàm `GetDescendantsDtoAsync` trong Repository của bạn.
         * Hàm này cực kỳ mạnh mẽ vì nó dùng OrgNode.IsDescendantOf để quét toàn bộ con cháu.
         */
        searchInSubtree: (
            id: number,
            keyword: string,
            pageIndex = 1,
            pageSize = 100,
            signal?: AbortSignal
        ) =>
            api.get<PagedResult<DepartmentDto>>(`/Departments/${id}/descendants`, {
                signal,
                params: {
                    PageIndex: pageIndex,
                    PageSize: pageSize,
                    NameContains: keyword.trim(),
                },
            }),

        /**
         * Lấy tất cả descendants trong 1 nhánh (không filter theo tên).
         * Dùng làm fallback khi endpoint /children trả rỗng nhưng node vẫn báo hasChildren=true.
         */
        getDescendants: (id: number, pageIndex = 1, pageSize = 100, signal?: AbortSignal) =>
            api.get<PagedResult<DepartmentDto>>(`/Departments/${id}/descendants`, {
                signal,
                params: {
                    PageIndex: pageIndex,
                    PageSize: pageSize,
                },
            }),

        /**
         * Lấy danh sách con trực tiếp (F1) - Dùng cho Lazy Loading cây
         * Gọi đến: [HttpGet("{id:int}/children")]
         * Sử dụng hàm `GetChildrenDtoAsync` trong Repository.
         */
        getChildren: (id: number) =>
            api.get<DepartmentDto[]>(`/Departments/${id}/children`),

        /**
         * Lấy chi tiết thông tin một phòng ban
         * Gọi đến: [HttpGet("{id:int}")]
         */
        getById: (id: number) =>
            api.get<DepartmentDto>(`/Departments/${id}`),


        // ============================================================
        // ✍️ NHÓM THAY ĐỔI DỮ LIỆU (WRITE)
        // ============================================================

        /**
         * Tạo mới phòng ban
         * Gọi đến: [HttpPost]
         * Repository sẽ dùng `GenerateNewNodeAsync` để tính toán OrgNode mới.
         */
        create: (data: CreateDepartmentRequest) =>
            api.post<DepartmentDto>("/Departments", data),

        /**
         * Cập nhật thông tin phòng ban
         * Gọi đến: [HttpPut("{id:int}")]
         */
        update: (id: number, data: UpdateDepartmentRequest) =>
            api.put<DepartmentDto>(`/Departments/${id}`, data),

        /**
         * Di chuyển phòng ban (Đổi cha)
         * Gọi đến: [HttpPut("{id:int}/move")]
         * Logic Repository sẽ dùng `ReparentSubtreeAsync` để cập nhật lại toàn bộ OrgNode của nhánh con.
         */
        move: (id: number, newParentId: number | null) =>
            api.put(`/Departments/${id}/move`, null, {
                params: { newParentId },
            }),

        /**
         * Xóa phòng ban
         * Gọi đến: [HttpDelete("{id:int}")]
         * Thường sẽ kiểm tra HasEmployeesAsync hoặc HasChildrenAsync trước khi cho xóa.
         */
        delete: (id: number) =>
            api.delete(`/Departments/${id}`),

        getChildrenPaged: async (id: number, pageIndex = 1, pageSize = 10) => {
            const res = await api.get<DepartmentDto[]>(`/Departments/${id}/children`);
            return {
                ...res,
                data: {
                    items: res.data,
                    totalCount: res.data.length,
                    pageIndex,
                    pageSize,
                    totalPages: 1,
                    hasNext: false,
                    hasPrevious: false,
                } as PagedResult<DepartmentDto>,
            };
        },

        /**
         * Tìm kiếm trong subtree cụ thể
         */

    };