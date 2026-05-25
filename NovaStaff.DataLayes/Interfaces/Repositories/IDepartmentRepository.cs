// DataLayers/Interfaces/Repositories/IDepartmentRepository.cs
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Filters;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository đặc thù cho Department — sơ đồ tổ chức công ty.
/// Sử dụng Materialized Path (string) cho cây phòng ban thay vì HierarchyId.
/// </summary>
public interface IDepartmentRepository : IRepository<Department, int>
{
    // =========================================================
    // VALIDATION SUPPORT
    // =========================================================

    Task<bool> HasEmployeesAsync(int departmentId, CancellationToken ct = default);

    Task<bool> HasDescendantsAsync(int departmentId, CancellationToken ct = default);

    Task<bool> HasChildrenAsync(int departmentId, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default);

    // =========================================================
    // HIERARCHY READ SUPPORT (Tính toán node mới)
    // =========================================================

    Task<string?> GetPositionAsync(int departmentId, CancellationToken ct = default);

    Task<string?> GetLastRootNodeAsync(CancellationToken ct = default);

    Task<string?> GetLastChildNodeAsync(string parentPath, CancellationToken ct = default);

    Task<(string newNode, string? parentNode)> GenerateNewNodeAsync(
        int? parentId,
        CancellationToken ct = default);

    // =========================================================
    // TREE QUERY (Entity — phục vụ mutation logic)
    // =========================================================

    Task<IReadOnlyList<Department>> GetChildrenAsync(int parentId, CancellationToken ct = default);

    // =========================================================
    // MANAGER SUPPORT
    // =========================================================

    Task<IReadOnlyList<DepartmentDto>> GetByManagerAsync(int managerEmployeeId, CancellationToken ct = default);

    // =========================================================
    // DTO PROJECTION (Tối ưu UI — không tracking)
    // =========================================================

    Task<DepartmentDto?> GetDtoByIdAsync(int id, CancellationToken ct = default);

    Task<PagedResult<DepartmentDto>> GetRootsDtoAsync(
        DepartmentDescendantFilter filter,
        int pageIndex,
        int pageSize,
        int? managerId = null,
        CancellationToken ct = default);

    Task<PagedResult<DepartmentDto>> GetDescendantsDtoAsync(
        int departmentId,
        DepartmentDescendantFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<DepartmentDto>> GetChildrenDtoAsync(int parentId, CancellationToken ct = default);

    // =========================================================
    // TREE MUTATION
    // =========================================================

    Task ReparentSubtreeAsync(string oldPath, string newPath, CancellationToken ct = default);

    Task<List<Department>> GetManagedDepartmentsAsync(
        int managerEmployeeId,
        CancellationToken ct = default);

    Task<List<int>> GetDescendantIdsAsync(
        IEnumerable<int> rootDepartmentIds,
        CancellationToken ct = default);
}
