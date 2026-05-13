// DataLayers/Interfaces/Repositories/IDepartmentRepository.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Filters;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository ð?c thù cho Department — sõ ð? t? ch?c công ty.
///
/// B?ng này s? d?ng HierarchyId c?a SQL Server. Thay v? dùng ParentId
/// và ph?i query ð? quy (CTE) ch?m ch?p, HierarchyId lýu ðý?ng d?n node (VD: /1/2/1/)
/// giúp query cha-con, h? hàng c?c nhanh ch? b?ng các hàm có s?n c?a SQL.
///
/// Các field quan tr?ng:
///   DepartmentID      : int, khoá chính (t? tãng)
///   OrgNode           : HierarchyId, to? ð? c?a ph?ng ban trong cây
///   OrgLevel          : short?, computed column (OrgNode.GetLevel())
///   ManagerEmployeeID : int?, FK ? Employee.EmployeeID
///   IsActive          : bool, tr?ng thái ho?t ð?ng
/// </summary>
public interface IDepartmentRepository : IRepository<Department, int>
{
    // =========================================================
    // VALIDATION SUPPORT
    // =========================================================

    /// <summary>
    /// Ki?m tra ph?ng ban có ch?a nhân viên (chýa b? xóa) không.
    ///
    /// SQL: SELECT TOP 1 1 FROM Employees WHERE DepartmentID = @id AND IsDeleted = 0
    ///
    /// Dùng khi: Ch?n xóa ph?ng ban c?n nhân viên.
    /// </summary>
    Task<bool> HasEmployeesAsync(int departmentId, CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra ph?ng ban có ph?ng ban con (cháu, ch?t...) không.
    ///
    /// SQL: SELECT TOP 1 1 FROM Departments
    ///      WHERE OrgNode.IsDescendantOf(@node) = 1 AND DepartmentID != @id AND IsDeleted = 0
    ///
    /// Dùng khi: Ch?n xóa ph?ng ban c?n ph?ng ban c?p dý?i.
    /// Thay th? vi?c g?i GetDescendantsPagedAsync ch? ð? check TotalCount > 0.
    /// </summary>
    Task<bool> HasDescendantsAsync(int departmentId, CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra ph?ng ban có con tr?c ti?p (F1) không.
    ///
    /// SQL: SELECT TOP 1 1 FROM Departments
    ///      WHERE OrgNode.GetAncestor(1) = @parentNode AND IsDeleted = 0
    ///
    /// Dùng khi: Ki?m tra nhanh trý?c khi x? l? logic cha-con.
    /// </summary>
    Task<bool> HasChildrenAsync(int departmentId, CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra Code ð? t?n t?i chýa (type-safe, tránh predicate leak).
    ///
    /// SQL: SELECT TOP 1 1 FROM Departments
    ///      WHERE Code = @code AND DepartmentID != @excludeId AND IsDeleted = 0
    ///
    /// Dùng khi: Validate unique Code trý?c Create/Update.
    /// Lýu ?: Luôn c?n unique index ? DB làm lý?i an toàn cu?i (race condition).
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default);

    // =========================================================
    // HIERARCHY READ SUPPORT (Tính toán node m?i)
    // =========================================================

    /// <summary>
    /// L?y to? ð? (OrgNode) hi?n t?i c?a m?t ph?ng ban.
    ///
    /// Dùng khi: C?n m?c OrgNode c?a parent ð? sinh OrgNode cho ph?ng ban con.
    /// </summary>
    Task<HierarchyId?> GetPositionAsync(int departmentId, CancellationToken ct = default);

    /// <summary>
    /// L?y OrgNode l?n nh?t ? Root Level (Level 1).
    ///
    /// SQL: SELECT TOP 1 OrgNode FROM Departments
    ///      WHERE OrgNode.GetLevel() = 1 AND IsDeleted = 0 ORDER BY OrgNode DESC
    ///
    /// Dùng khi: T?o node g?c m?i — l?y node l?n nh?t hi?n t?i (/3/) ð? sinh (/4/).
    /// </summary>
    Task<HierarchyId?> GetLastRootNodeAsync(CancellationToken ct = default);

    /// <summary>
    /// L?y OrgNode con l?n nh?t c?a m?t Parent.
    ///
    /// Dùng khi: T?o ph?ng ban con — l?y sibling cu?i (/1/2/) ð? sinh (/1/3/).
    /// </summary>
    Task<HierarchyId?> GetLastChildNodeAsync(HierarchyId parentNode, CancellationToken ct = default);

    /// <summary>
    /// Sinh OrgNode m?i cho m?t ph?ng ban (t?p trung logic t?i Repository).
    ///
    /// Logic:
    ///   - parentId == null ? Root node m?i (GetLastRootNodeAsync ? GetDescendant)
    ///   - parentId != null ? Child node m?i (GetPositionAsync ? GetLastChildNodeAsync ? GetDescendant)
    ///
    /// Dùng khi: CreateAsync và MoveAsync — gom 2–3 query thành 1 method r? ràng.
    /// Ném KeyNotFoundException n?u parentId không t?n t?i.
    /// </summary>
    Task<(HierarchyId newNode, HierarchyId? parentNode)> GenerateNewNodeAsync(
        int? parentId,
        CancellationToken ct = default);

    // =========================================================
    // TREE QUERY (Entity — ph?c v? mutation logic)
    // =========================================================

    /// <summary>
    /// L?y danh sách con tr?c ti?p (F1) d?ng Entity.
    ///
    /// SQL: SELECT * FROM Departments
    ///      WHERE OrgNode.GetAncestor(1) = @parentNode AND IsDeleted = 0
    ///
    /// Dùng khi: Ki?m tra logic trý?c khi Update/Delete node cha.
    /// </summary>
    Task<IReadOnlyList<Department>> GetChildrenAsync(int parentId, CancellationToken ct = default);

    // =========================================================
    // MANAGER SUPPORT
    // =========================================================

    /// <summary>
    /// L?y các ph?ng ban do m?t nhân s? qu?n l? (DTO).
    ///
    /// Dùng khi: Dashboard Manager — "Các ph?ng ban tôi qu?n l?".
    /// Tr? v? DTO thay v? Entity ð? nh?t quán v?i các read method khác.
    /// </summary>
    Task<IReadOnlyList<DepartmentDto>> GetByManagerAsync(int managerEmployeeId, CancellationToken ct = default);

    // =========================================================
    // DTO PROJECTION (T?i ýu UI — không tracking)
    // =========================================================

    /// <summary>
    /// L?y chi ti?t ph?ng ban (DTO), JOIN ng?m ra Employee ð? l?y ManagerName.
    ///
    /// Dùng khi: UI g?i GetById ð? load form s?a thông tin.
    /// </summary>
    Task<DepartmentDto?> GetDtoByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// L?y danh sách ph?ng ban g?c (Level 1) d?ng DTO, có phân trang và filter.
    ///
    /// Dùng khi: L?n ð?u vào trang "Sõ ð? t? ch?c" — ch? load Level 1,
    /// tránh load c? cây ngàn node làm treo tr?nh duy?t.
    /// </summary>
    Task<PagedResult<DepartmentDto>> GetRootsDtoAsync(
        DepartmentDescendantFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// L?y danh sách c?p dý?i (DTO) v?i phân trang và filter.
    ///
    /// Dùng khi: Thanh search trên màn h?nh qu?n l? — t?m theo Code/Name.
    /// </summary>
    Task<PagedResult<DepartmentDto>> GetDescendantsDtoAsync(
        int departmentId,
        DepartmentDescendantFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// L?y danh sách con tr?c ti?p (F1) d?ng DTO.
    ///
    /// Dùng khi: TreeView lazy load — user b?m [+] m? r?ng node,
    /// ch? fetch ðúng con c?a node ðó, nh? và mý?t.
    /// </summary>
    Task<IReadOnlyList<DepartmentDto>> GetChildrenDtoAsync(int parentId, CancellationToken ct = default);

    // =========================================================
    // TREE MUTATION
    // =========================================================

    /// <summary>
    /// Reparent toàn b? subtree b?ng SQL Server function (atomic).
    ///
    /// SQL: UPDATE Departments
    ///      SET OrgNode = OrgNode.GetReparentedValue(@old, @new)
    ///      WHERE OrgNode.IsDescendantOf(@old) = 1
    ///
    /// Dùng khi: MoveAsync — ph?i ch?y trong transaction Serializable.
    /// </summary>
    Task ReparentSubtreeAsync(HierarchyId oldNode, HierarchyId newNode, CancellationToken ct = default);
}



