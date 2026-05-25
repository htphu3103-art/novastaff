// Services/Interfaces/IDepartmentService.cs
using NovaStaff.BusinessLayers.DTOs.Departments;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Filters;

namespace NovaStaff.Services.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<DepartmentDto>> GetDescendantsAsync(int departmentId, DepartmentDescendantQuery query, CancellationToken ct = default);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// L?y danh sách ph?ng ban con tr?c ti?p (level +1).
    /// Query:
    ///   WHERE OrgNode.GetAncestor(1) = @parentNode
    /// </summary>
    Task<IReadOnlyList<DepartmentDto>> GetChildrenAsync(
    int parentId,
    CancellationToken ct = default);

    Task<PagedResult<DepartmentDto>> GetRootsAsync(
    DepartmentDescendantQuery query,
    CancellationToken ct = default);

    Task MoveAsync(int id, int? newParentId, CancellationToken ct = default);

}



