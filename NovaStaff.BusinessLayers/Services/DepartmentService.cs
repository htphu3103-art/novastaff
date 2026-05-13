using NovaStaff.BusinessLayers.DTOs.Departments;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;
using NovaStaff.Models.Enums;
using System.Data;

namespace NovaStaff.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IDepartmentRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;  // ✅ NEW

    public DepartmentService(
        IUnitOfWork uow,
        IDepartmentRepository repo,
        IEmployeeRepository employeeRepo)  // ✅ Inject AuditService
    {
        _uow = uow;
        _repo = repo;
        _employeeRepo = employeeRepo;
       
    }

    // =========================================================
    // READ - Giữ nguyên (không cần audit)
    // =========================================================
    public async Task<DepartmentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _repo.GetDtoByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Department {id} không tồn tại.");
    }

    public async Task<PagedResult<DepartmentDto>> GetRootsAsync(
        DepartmentDescendantQuery query,
        CancellationToken ct = default)
    {
        return await _repo.GetRootsDtoAsync(
            ToFilter(query),
            query.PageIndex,
            query.PageSize,
            ct);
    }

    public async Task<PagedResult<DepartmentDto>> GetDescendantsAsync(
        int departmentId,
        DepartmentDescendantQuery query,
        CancellationToken ct = default)
    {
        await EnsureExistsAsync(departmentId, ct);
        return await _repo.GetDescendantsDtoAsync(
            departmentId,
            ToFilter(query),
            query.PageIndex,
            query.PageSize,
            ct);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetChildrenAsync(
        int parentId,
        CancellationToken ct = default)
    {
        await EnsureExistsAsync(parentId, ct);
        return await _repo.GetChildrenDtoAsync(parentId, ct);
    }

    // =========================================================
    // CREATE ✅ + BUSINESS AUDIT
    // =========================================================
    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken ct = default)
    {
        ValidateName(request.Name);
        await ValidateCodeUniqueAsync(request.Code, excludeId: null, ct);
        await ValidateManagerAsync(request.ManagerEmployeeId, ct);

        await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var (newNode, _) = await _repo.GenerateNewNodeAsync(request.ParentId, ct);

            var dept = new Department
            {
                DepartmentName = request.Name.Trim(),
                Code = NormalizeCode(request.Code),
                OrgNode = newNode,
                Description = request.Description?.Trim(),
                IsActive = true,
                ManagerEmployeeID = request.ManagerEmployeeId
            };

            await _repo.AddAsync(dept, ct);
            await _uow.SaveChangesAsync(ct);  // ✅ Tech Audit (Interceptor)

            // ✅ BUSINESS AUDIT
            //await _audit.LogAsync("Departments", $"NEW|{dept.DepartmentID}", AuditAction.Insert, null, request);

            await tx.CommitAsync(ct);

            return await _repo.GetDtoByIdAsync(dept.DepartmentID, ct)
                ?? throw new InvalidOperationException("Failed to load created department.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================================================
    // UPDATE ✅ + BUSINESS AUDIT
    // =========================================================
    public async Task<DepartmentDto> UpdateAsync(
    int id,
    UpdateDepartmentRequest request,
    CancellationToken ct = default)
    {
        var oldDept = await _repo.GetDtoByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Department {id} không tồn tại.");

        ValidateName(request.Name);
        await ValidateCodeUniqueAsync(request.Code, excludeId: id, ct);
        await ValidateManagerAsync(request.ManagerEmployeeId, ct);

        var dept = (await _repo.GetByIdAsync(id, trackChanges: true, ct: ct))!;
        dept.DepartmentName = request.Name.Trim();
        dept.Code = NormalizeCode(request.Code);
        dept.Description = request.Description?.Trim();
        dept.IsActive = request.IsActive;
        dept.ManagerEmployeeID = request.ManagerEmployeeId;

        _repo.Update(dept);
        await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi

        return await _repo.GetDtoByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Failed to load updated department.");
    }

    // =========================================================
    // DELETE ✅ + BUSINESS AUDIT
    // =========================================================
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var dept = await _repo.GetByIdAsync(id, trackChanges: true, ct: ct)
            ?? throw new KeyNotFoundException($"Department {id} không tồn tại.");

        await EnsureNoEmployeesAsync(dept.DepartmentID, dept.DepartmentName, ct);
        await EnsureNoDescendantsAsync(dept.DepartmentID, dept.DepartmentName, ct);

        _repo.Delete(dept);
        await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi
    }

    // =========================================================
    // MOVE ✅ + BUSINESS AUDIT
    // =========================================================
    public async Task MoveAsync(int id, int? newParentId, CancellationToken ct = default)
    {
        var currentNode = await _repo.GetPositionAsync(id, ct)
            ?? throw new KeyNotFoundException($"Department {id} không tồn tại.");

        await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var (newNode, newParentNode) = await _repo.GenerateNewNodeAsync(newParentId, ct);

            if (newParentId == id ||
                (newParentNode != null && newParentNode.IsDescendantOf(currentNode)))
                throw new InvalidOperationException(
                    "Không thể move phòng ban vào chính nó hoặc vào cây con của nó.");

            await _repo.ReparentSubtreeAsync(currentNode, newNode, ct);
            await _uow.SaveChangesAsync(ct); // ← Interceptor tự ghi

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================================================
    // PRIVATE HELPERS - Giữ nguyên
    // =========================================================
    private async Task EnsureExistsAsync(int id, CancellationToken ct)
    {
        if (!await _repo.ExistsAsync(id, ct))
            throw new KeyNotFoundException($"Department {id} không tồn tại.");
    }

    private async Task EnsureNoEmployeesAsync(int deptId, string deptName, CancellationToken ct)
    {
        if (await _repo.HasEmployeesAsync(deptId, ct))
            throw new InvalidOperationException(
                $"Không thể xóa '{deptName}' vì còn nhân viên. Vui lòng thuyên chuyển nhân viên trước.");
    }

    private async Task EnsureNoDescendantsAsync(int deptId, string deptName, CancellationToken ct)
    {
        if (await _repo.HasDescendantsAsync(deptId, ct))
            throw new InvalidOperationException(
                $"Không thể xóa '{deptName}' vì còn phòng ban cấp dưới. Vui lòng xóa hoặc di chuyển phòng ban con trước.");
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên phòng ban không được để trống.");
    }

    private async Task ValidateCodeUniqueAsync(string? code, int? excludeId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        if (await _repo.CodeExistsAsync(code, excludeId, ct))
            throw new InvalidOperationException(
                $"Code '{NormalizeCode(code)}' đã tồn tại. Vui lòng chọn code khác.");
    }

    private async Task ValidateManagerAsync(int? managerId, CancellationToken ct)
    {
        if (!managerId.HasValue) return;

        if (!await _employeeRepo.ExistsAsync(managerId.Value, ct))
            throw new KeyNotFoundException(
                $"Nhân viên {managerId} không tồn tại hoặc đã bị xóa.");
    }

    private static string? NormalizeCode(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : code.Trim().ToUpperInvariant();

    private static DepartmentDescendantFilter ToFilter(DepartmentDescendantQuery query)
        => new()
        {
            NameContains = query.NameContains,
            IsActive = query.IsActive,
            ManagerId = query.ManagerId,
            SortBy = query.SortBy,
            SortDescending = query.SortDescending
        };
}