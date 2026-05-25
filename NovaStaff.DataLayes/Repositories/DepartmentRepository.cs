// DataLayers/Repositories/DepartmentRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NovaStaff.DataLayers.Repositories;

public class DepartmentRepository : GenericRepository<Department, int>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    // =========================================================
    // MAPPING EXPRESSION
    // =========================================================

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
                .Where(x => d.OrgLevel > 1 &&
                            x.OrgLevel == d.OrgLevel - 1 &&
                            d.OrgPath.StartsWith(x.OrgPath))
                .Select(x => (int?)x.DepartmentID)
                .FirstOrDefault(),

            HasChildren = includeHasChildren
                ? _context.Departments
                    .Any(x => x.OrgLevel == d.OrgLevel + 1 &&
                              x.OrgPath.StartsWith(d.OrgPath))
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
        return await _context.Employees
            .AnyAsync(e => e.DepartmentID == departmentId, ct);
    }

    public async Task<bool> HasDescendantsAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        var path = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgPath)
            .FirstOrDefaultAsync(ct);

        if (path == null) return false;

        return await _dbSet.AnyAsync(d =>
            d.OrgPath.StartsWith(path) &&
            d.DepartmentID != departmentId, ct);
    }

    public async Task<bool> HasChildrenAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        var dep = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => new { d.OrgPath, d.OrgLevel })
            .FirstOrDefaultAsync(ct);

        if (dep == null) return false;

        return await _dbSet.AnyAsync(d =>
            d.OrgLevel == dep.OrgLevel + 1 &&
            d.OrgPath.StartsWith(dep.OrgPath), ct);
    }

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

    public async Task<string?> GetPositionAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgPath)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetLastRootNodeAsync(CancellationToken ct = default)
    {
        var rootPaths = await _dbSet
            .Where(d => d.OrgLevel == 1)
            .Select(d => d.OrgPath)
            .ToListAsync(ct);

        if (!rootPaths.Any()) return null;

        return rootPaths
            .OrderByDescending(path =>
            {
                var segment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                return int.TryParse(segment, out int val) ? val : 0;
            })
            .FirstOrDefault();
    }

    public async Task<string?> GetLastChildNodeAsync(
        string parentPath,
        CancellationToken ct = default)
    {
        var childPaths = await _dbSet
            .Where(d => d.OrgLevel == (short?)(parentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length + 1) &&
                        d.OrgPath.StartsWith(parentPath))
            .Select(d => d.OrgPath)
            .ToListAsync(ct);

        if (!childPaths.Any()) return null;

        return childPaths
            .OrderByDescending(path =>
            {
                var segment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                return int.TryParse(segment, out int val) ? val : 0;
            })
            .FirstOrDefault();
    }

    public async Task<(string newNode, string? parentNode)> GenerateNewNodeAsync(
        int? parentId,
        CancellationToken ct = default)
    {
        if (parentId is null)
        {
            var lastRoot = await GetLastRootNodeAsync(ct);
            return (GetDescendant("/", lastRoot), null);
        }

        var parentPath = await GetPositionAsync(parentId.Value, ct)
            ?? throw new KeyNotFoundException($"Parent department {parentId} không tồn tại.");

        var lastChild = await GetLastChildNodeAsync(parentPath, ct);
        return (GetDescendant(parentPath, lastChild), parentPath);
    }

    private static string GetDescendant(string parentPath, string? lastChildPath)
    {
        if (string.IsNullOrEmpty(parentPath)) parentPath = "/";
        if (!parentPath.StartsWith("/")) parentPath = "/" + parentPath;
        if (!parentPath.EndsWith("/")) parentPath = parentPath + "/";

        if (lastChildPath != null)
        {
            if (!lastChildPath.StartsWith("/")) lastChildPath = "/" + lastChildPath;
            if (!lastChildPath.EndsWith("/")) lastChildPath = lastChildPath + "/";
        }

        if (lastChildPath == null)
        {
            return $"{parentPath}1/";
        }
        else
        {
            var segments = lastChildPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return $"{parentPath}1/";
            }
            var lastSegment = segments[^1];
            if (int.TryParse(lastSegment, out int lastNum))
            {
                int nextNum = lastNum + 1;
                return $"{parentPath}{nextNum}/";
            }
            else
            {
                return $"{parentPath}1/";
            }
        }
    }

    // =========================================================
    // TREE QUERY (Entity — phục vụ mutation logic)
    // =========================================================

    public async Task<IReadOnlyList<Department>> GetChildrenAsync(
        int parentId,
        CancellationToken ct = default)
    {
        var parentPath = await _dbSet
            .Where(d => d.DepartmentID == parentId)
            .Select(d => d.OrgPath)
            .FirstOrDefaultAsync(ct);

        if (parentPath == null) return Array.Empty<Department>();

        var parentLevel = parentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

        return await _dbSet
            .AsNoTracking()
            .Where(d => d.OrgLevel == parentLevel + 1 && d.OrgPath.StartsWith(parentPath))
            .OrderBy(d => d.OrgPath)
            .ToListAsync(ct);
    }

    // =========================================================
    // MANAGER SUPPORT
    // =========================================================

    public async Task<IReadOnlyList<DepartmentDto>> GetByManagerAsync(
        int managerEmployeeId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.ManagerEmployeeID == managerEmployeeId)
            .OrderBy(d => d.OrgPath)
            .Select(MapToDto(includeHasChildren: true))
            .ToListAsync(ct);
    }

    // =========================================================
    // DTO PROJECTION
    // =========================================================

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

    public async Task<IReadOnlyList<DepartmentDto>> GetChildrenDtoAsync(
        int parentId,
        CancellationToken ct = default)
    {
        var parentPath = await _dbSet
            .Where(d => d.DepartmentID == parentId)
            .Select(d => d.OrgPath)
            .FirstOrDefaultAsync(ct);

        if (parentPath == null) return Array.Empty<DepartmentDto>();

        var parentLevel = parentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

        return await _dbSet
            .AsNoTracking()
            .Where(d => d.OrgLevel == parentLevel + 1 && d.OrgPath.StartsWith(parentPath))
            .OrderBy(d => d.OrgPath)
            .Select(MapToDto(includeHasChildren: true))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<DepartmentDto>> GetDescendantsDtoAsync(
    int departmentId,
    DepartmentDescendantFilter filter,
    int pageIndex,
    int pageSize,
    CancellationToken ct = default)
    {
        var parentPath = await _dbSet
            .Where(d => d.DepartmentID == departmentId)
            .Select(d => d.OrgPath)
            .FirstOrDefaultAsync(ct);

        if (parentPath == null)
            return PagedResult<DepartmentDto>.Empty(pageIndex, pageSize);

        var query = _dbSet
            .AsNoTracking()
            .Where(d =>
                d.OrgPath.StartsWith(parentPath) &&
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

        var ids = items.Select(x => x.Id).ToList();

        var nodeInfos = await _dbSet
            .Where(d => ids.Contains(d.DepartmentID))
            .Select(d => new { d.DepartmentID, d.OrgPath, d.OrgLevel })
            .ToListAsync(ct);

        var nodeValues = nodeInfos.Select(x => x.OrgPath).ToList();
        var parentLevelMap = nodeInfos.ToDictionary(x => x.OrgPath, x => x.OrgLevel);
        
        var hasChildrenPaths = new HashSet<string>();
        foreach (var path in nodeValues)
        {
            var level = parentLevelMap[path];
            var hasChild = await _dbSet.AnyAsync(d => d.OrgLevel == level + 1 && d.OrgPath.StartsWith(path), ct);
            if (hasChild)
            {
                hasChildrenPaths.Add(path);
            }
        }

        foreach (var item in items)
        {
            var info = nodeInfos.FirstOrDefault(x => x.DepartmentID == item.Id);
            if (info != null)
            {
                item.HasChildren = hasChildrenPaths.Contains(info.OrgPath);
            }
        }

        return new PagedResult<DepartmentDto>(items, total, pageIndex, pageSize);
    }

    public async Task<PagedResult<DepartmentDto>> GetRootsDtoAsync(
    DepartmentDescendantFilter filter,
    int pageIndex,
    int pageSize,
    int? managerId = null,
    CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(d => d.OrgLevel == 1);

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

        if (items.Count > 0)
        {
            var ids = items.Select(x => x.Id).ToList();
            var nodeInfos = await _dbSet
                .Where(d => ids.Contains(d.DepartmentID))
                .Select(d => new { d.DepartmentID, d.OrgPath })
                .ToListAsync(ct);

            var hasChildrenPaths = new HashSet<string>();
            foreach (var info in nodeInfos)
            {
                var hasChild = await _dbSet.AnyAsync(d => d.OrgLevel == 2 && d.OrgPath.StartsWith(info.OrgPath), ct);
                if (hasChild)
                {
                    hasChildrenPaths.Add(info.OrgPath);
                }
            }

            foreach (var item in items)
            {
                var info = nodeInfos.FirstOrDefault(x => x.DepartmentID == item.Id);
                if (info != null)
                {
                    item.HasChildren = hasChildrenPaths.Contains(info.OrgPath);
                }
            }
        }

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
        string oldPath,
        string newPath,
        CancellationToken ct = default)
    {
        var oldLevel = oldPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
        var newLevel = newPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
        var levelOffset = newLevel - oldLevel;

        await _context.Database.ExecuteSqlRawAsync(@"
            UPDATE ""Departments""
            SET ""OrgPath"" = {0} || SUBSTRING(""OrgPath"" FROM LENGTH({1}) + 1),
                ""OrgLevel"" = CAST(""OrgLevel"" + {2} AS smallint)
            WHERE ""OrgPath"" LIKE {3}
        ", new object[] { newPath, oldPath, levelOffset, oldPath + "%" }, ct);
    }

    // =========================================================
    // PRIVATE HELPERS
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
            (_, false) => query.OrderBy(d => d.OrgPath),
            (_, true) => query.OrderByDescending(d => d.OrgPath)
        };
    }

    public async Task<List<Department>> GetManagedDepartmentsAsync(
        int managerEmployeeId,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.ManagerEmployeeID == managerEmployeeId)
            .OrderBy(d => d.OrgPath)
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

        var rootPaths = await _dbSet
            .Where(d => roots.Contains(d.DepartmentID))
            .Select(d => d.OrgPath)
            .ToListAsync(ct);

        var allDeps = await _dbSet
            .AsNoTracking()
            .Select(d => new { d.DepartmentID, d.OrgPath })
            .ToListAsync(ct);

        var descendantIds = new List<int>();
        foreach (var dep in allDeps)
        {
            if (roots.Contains(dep.DepartmentID)) continue;
            if (rootPaths.Any(root => dep.OrgPath.StartsWith(root)))
            {
                descendantIds.Add(dep.DepartmentID);
            }
        }

        return descendantIds;
    }
}
