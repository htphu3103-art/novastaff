// DataLayers/Repositories/DepartmentRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Filters;
using System.Linq.Expressions;
namespace NovaStaff.DataLayers.Repositories;

public class DepartmentRepository : GenericRepository<Department, int>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    // =========================================================
    // MAPPING EXPRESSION
    // =========================================================

    /// <summary>
    /// Map Department ? DepartmentDto.
    ///
    /// includeHasChildren = false (default):
    ///   B? qua subquery HasChildren — dùng cho list/paging l?n
    ///   đ? tránh N EXISTS subquery per row gây bottleneck.
    ///
    /// includeHasChildren = true:
    ///   B?t subquery EXISTS — ch? dùng khi result set nh?:
    ///   GetById (1 row), GetChildren (F1 c?a 1 node).
    ///
    /// EF Core d?ch HasChildren thành EXISTS inline trong cùng câu SQL,
    /// nhưng v?n là per-row cost ? ki?m soát ch?t qua flag này.
    /// </summary>
    private Expression<Func<Department, DepartmentDto>> MapToDto(bool includeHasChildren = false)
    {
        return d => new DepartmentDto
        {
            Id = d.DepartmentID,
            Name = d.DepartmentName,
            Code = d.Code,
            Level = d.OrgLevel,
            IsActive = d.IsActive,
            Description = d.Description,
            ManagerId = d.ManagerEmployeeID,
            ManagerName = d.Manager != null ? d.Manager.FullName : null,

            ParentId = _context.Departments
                .Where(x => d.OrgNode.GetLevel() > 1 &&
                            x.OrgNode == d.OrgNode.GetAncestor(1))
                .Select(x => (int?)x.DepartmentID)
                .FirstOrDefault(),

            HasChildren = includeHasChildren
                ? _context.Departments
                    .Any(x => x.OrgNode.GetAncestor(1) == d.OrgNode)
                : null
        };
    }

    // =========================================================
    // VALIDATION SUPPORT
    // =========================================================

    public async Task<bool> HasEmployeesAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        // Global filter Employee (IsDeleted = 0) t? đ?ng áp d?ng
        return await _context.Employees
            .AnyAsync(e => e.DepartmentID == departmentId, ct);
    }

    public async Task<bool> HasDescendantsAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        var node = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);

        if (node == null) return false;

        return await _dbSet.AnyAsync(d =>
            d.OrgNode.IsDescendantOf(node) &&
            d.DepartmentID != departmentId, ct);
    }

    public async Task<bool> HasChildrenAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        var node = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);

        if (node == null) return false;

        return await _dbSet.AnyAsync(d => d.OrgNode.GetAncestor(1) == node, ct);
    }

    /// <summary>
    /// Repo assume normalizedCode đ? đư?c chu?n hóa b?i Service (Trim + ToUpperInvariant).
    /// Không normalize l?i ? đây — single source of truth ? Service.
    /// </summary>
    public async Task<bool> CodeExistsAsync(
        string normalizedCode,
        int? excludeId,
        CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(d =>
            d.Code == normalizedCode &&
            (!excludeId.HasValue || d.DepartmentID != excludeId.Value), ct);
    }

    // =========================================================
    // HIERARCHY READ SUPPORT
    // =========================================================

    public async Task<HierarchyId?> GetPositionAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<HierarchyId?> GetLastRootNodeAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(d => d.OrgNode.GetLevel() == 1)
            .OrderByDescending(d => d.OrgNode)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<HierarchyId?> GetLastChildNodeAsync(
        HierarchyId parentNode,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(d => d.OrgNode.GetAncestor(1) == parentNode)
            .OrderByDescending(d => d.OrgNode)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Sinh OrgNode m?i — t?p trung node logic t?i Repository.
    ///
    /// Root: 1 query (GetLastRootNodeAsync)
    /// Child: 2 query (GetPositionAsync + GetLastChildNodeAsync)
    ///
    /// N?u scale l?n (10k+ departments, concurrent cao), có th? g?p thành 1 raw SQL:
    ///   SELECT parent.OrgNode, MAX(child.OrgNode)
    ///   FROM Departments parent
    ///   LEFT JOIN Departments child ON child.OrgNode.GetAncestor(1) = parent.OrgNode
    ///   WHERE parent.DepartmentID = @id
    ///   GROUP BY parent.OrgNode
    ///
    /// Hi?n t?i 2 round-trip ch?p nh?n đư?c cho h? th?ng HRM t?m v?a.
    /// </summary>
    public async Task<(HierarchyId newNode, HierarchyId? parentNode)> GenerateNewNodeAsync(
        int? parentId,
        CancellationToken ct = default)
    {
        if (parentId is null)
        {
            var lastRoot = await GetLastRootNodeAsync(ct);
            return (HierarchyId.GetRoot().GetDescendant(lastRoot, null), null);
        }

        var parentNode = await GetPositionAsync(parentId.Value, ct)
            ?? throw new KeyNotFoundException($"Parent department {parentId} không t?n t?i.");

        var lastChild = await GetLastChildNodeAsync(parentNode, ct);
        return (parentNode.GetDescendant(lastChild, null), parentNode);
    }

    // =========================================================
    // TREE QUERY (Entity — ph?c v? mutation logic)
    // =========================================================

    public async Task<IReadOnlyList<Department>> GetChildrenAsync(
        int parentId,
        CancellationToken ct = default)
    {
        var parentNode = await _dbSet
            .Where(d => d.DepartmentID == parentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);

        if (parentNode == null) return Array.Empty<Department>();

        return await _dbSet
            .AsNoTracking()
            .Where(d => d.OrgNode.GetAncestor(1) == parentNode)
            .OrderBy(d => d.OrgNode)
            .ToListAsync(ct);
    }

    // =========================================================
    // MANAGER SUPPORT
    // =========================================================

    /// <summary>
    /// Manager thư?ng qu?n l? ít ph?ng ban ? t?p nh? ? b?t HasChildren an toàn.
    /// </summary>
    public async Task<IReadOnlyList<DepartmentDto>> GetByManagerAsync(
        int managerEmployeeId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.ManagerEmployeeID == managerEmployeeId)
            .OrderBy(d => d.OrgNode)
            .Select(MapToDto(includeHasChildren: true))
            .ToListAsync(ct);
    }

    // =========================================================
    // DTO PROJECTION
    // =========================================================

    /// <summary>GetById ? 1 row ? b?t HasChildren, không có per-row cost đáng k?.</summary>
    public async Task<DepartmentDto?> GetDtoByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.DepartmentID == id)
            .Select(MapToDto(includeHasChildren: true))
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>GetChildren (F1 c?a 1 node) ? t?p nh? ? b?t HasChildren.</summary>
    public async Task<IReadOnlyList<DepartmentDto>> GetChildrenDtoAsync(
        int parentId,
        CancellationToken ct = default)
    {
        var parentNode = await _dbSet
            .Where(d => d.DepartmentID == parentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);

        if (parentNode == null) return Array.Empty<DepartmentDto>();

        return await _dbSet
            .AsNoTracking()
            .Where(d => d.OrgNode.GetAncestor(1) == parentNode)
            .OrderBy(d => d.OrgNode)
            .Select(MapToDto(includeHasChildren: true))
            .ToListAsync(ct);
    }

    /// <summary>
    /// GetDescendants — paging l?n ? t?t HasChildren tránh N EXISTS subquery per row.
    /// </summary>
    public async Task<PagedResult<DepartmentDto>> GetDescendantsDtoAsync(
    int departmentId,
    DepartmentDescendantFilter filter,
    int pageIndex,
    int pageSize,
    CancellationToken ct = default)
    {
        var parentNode = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgNode)
            .FirstOrDefaultAsync(ct);

        if (parentNode == null)
            return PagedResult<DepartmentDto>.Empty(pageIndex, pageSize);

        var query = _dbSet
            .AsNoTracking()
            .Where(d =>
                d.OrgNode.IsDescendantOf(parentNode) &&
                d.DepartmentID != departmentId);

        query = ApplyFilter(query, filter);
        query = ApplySort(query, filter);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto(includeHasChildren: false))
            .ToListAsync(ct);

        if (items.Count == 0)
            return PagedResult<DepartmentDto>.Empty(pageIndex, pageSize);

        // ===== FIX HAS CHILDREN (SAFE EF VERSION) =====
        var ids = items.Select(x => x.Id).ToList();

        var nodeInfos = await _dbSet
            .Where(d => ids.Contains(d.DepartmentID))
            .Select(d => new { d.DepartmentID, d.OrgNode })
            .ToListAsync(ct);

        var nodeValues = nodeInfos.Select(x => x.OrgNode).ToList();

        var childrenNodes = await _dbSet
            .Where(d => d.OrgNode.GetAncestor(1) != null && nodeValues.Contains(d.OrgNode.GetAncestor(1)!))
            .Select(d => d.OrgNode.GetAncestor(1))
            .Distinct()
            .ToListAsync(ct);

        var map = nodeInfos.ToDictionary(x => x.DepartmentID, x => x.OrgNode);

        foreach (var item in items)
        {
            if (map.TryGetValue(item.Id, out var node))
            {
                item.HasChildren = childrenNodes.Contains(node);
            }
        }

        return new PagedResult<DepartmentDto>(items, total, pageIndex, pageSize);
    }

    /// <summary>GetRoots — paging ? t?t HasChildren, l? do như GetDescendants.</summary>
    public async Task<PagedResult<DepartmentDto>> GetRootsDtoAsync(
    DepartmentDescendantFilter filter,
    int pageIndex,
    int pageSize,
    int? managerId = null,
    CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(d => d.OrgNode.GetLevel() == 1);

        if (managerId.HasValue)
        {
            query = query.Where(d => d.ManagerEmployeeID == managerId.Value);
        }

        query = ApplyFilter(query, filter);

        var total = await query.CountAsync(ct);

        query = ApplySort(query, filter);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto(includeHasChildren: false))
            .ToListAsync(ct);

        return new PagedResult<DepartmentDto>(
            items,
            total,
            pageIndex,
            pageSize);
    }

    // =========================================================
    // TREE MUTATION
    // =========================================================

    public async Task ReparentSubtreeAsync(
        HierarchyId oldNode,
        HierarchyId newNode,
        CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Departments
            SET OrgNode = OrgNode.GetReparentedValue({oldNode}, {newNode})
            WHERE OrgNode.IsDescendantOf({oldNode}) = 1
        ", ct);
    }

    // =========================================================
    // PRIVATE HELPERS (DRY — tránh l?p filter/sort logic)
    // =========================================================

    private static IQueryable<Department> ApplyFilter(
        IQueryable<Department> query,
        DepartmentDescendantFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
            query = query.Where(d =>
                d.DepartmentName.Contains(filter.NameContains.Trim()));

        if (filter.IsActive.HasValue)
            query = query.Where(d => d.IsActive == filter.IsActive.Value);

        if (filter.ManagerId.HasValue)
            query = query.Where(d => d.ManagerEmployeeID == filter.ManagerId.Value);

        return query;
    }

    private static IQueryable<Department> ApplySort(
        IQueryable<Department> query,
        DepartmentDescendantFilter filter)
    {
        return (filter.SortBy, filter.SortDescending) switch
        {
            (DepartmentSortField.Name, false) => query.OrderBy(d => d.DepartmentName),
            (DepartmentSortField.Name, true) => query.OrderByDescending(d => d.DepartmentName),
            (DepartmentSortField.CreatedAt, false) => query.OrderBy(d => d.CreatedDate),
            (DepartmentSortField.CreatedAt, true) => query.OrderByDescending(d => d.CreatedDate),
            (_, false) => query.OrderBy(d => d.OrgNode),
            (_, true) => query.OrderByDescending(d => d.OrgNode)
        };
    }

    public async Task<List<Department>> GetManagedDepartmentsAsync(
    int managerEmployeeId,
    CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.ManagerEmployeeID == managerEmployeeId)
            .OrderBy(d => d.OrgNode)
            .ToListAsync(ct);
    }
    public async Task<List<int>> GetDescendantIdsAsync(
    IEnumerable<int> rootDepartmentIds,
    CancellationToken ct = default)
    {
        var roots = rootDepartmentIds
            .Distinct()
            .ToList();

        if (!roots.Any())
            return [];

        var rootNodes = await _dbSet
            .Where(d => roots.Contains(d.DepartmentID))
            .Select(d => d.OrgNode)
            .ToListAsync(ct);

        return await _dbSet
            .AsNoTracking()
            .Where(d =>
                rootNodes.Any(root =>
                    d.OrgNode.IsDescendantOf(root)) &&
                !roots.Contains(d.DepartmentID))
            .Select(d => d.DepartmentID)
            .Distinct()
            .ToListAsync(ct);
    }
}



