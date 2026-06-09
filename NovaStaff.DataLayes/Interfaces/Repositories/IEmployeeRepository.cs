// Interfaces/Repositories/IEmployeeRepository.cs
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

/// <summary>
/// Repository đ?c thù cho Employee — entity trung tâm c?a h? th?ng NovaStaff.
///
/// Employee liên k?t v?i h?u h?t entity khác:
///   DepartmentID   ? thu?c ph?ng ban nào (Department.DepartmentID)
///   SupervisorID   ? qu?n l? tr?c ti?p (self-reference Employee.EmployeeID)
///   User           ? tài kho?n đăng nh?p (1-1 v?i User.EmployeeID)
///   AttendanceRecords, PayrollDetails, LeaveRequests, WorkTasks ? 1-N
///
/// Ch? khai báo method mà IRepository generic không th? x? l?.
/// CRUD cơ b?n (GetByIdAsync dùng EmployeeID int,
/// GetPagedAsync, AddAsync, Update, Delete) đ? có s?n t? IRepository.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee, int>
{
    /// <summary>
    /// T?m nhân viên theo EmployeeCode — m? đ?nh danh nghi?p v? (ví d? "NV1001").
    ///
    /// EmployeeCode khác EmployeeID:
    ///   EmployeeID   : khoá chính DB, do PostgreSQL tự sinh (int, identity).
    ///   EmployeeCode : m? nghi?p v?, do sequence "EmployeeCodeSequence" sinh
    ///                  (StartsAt 1000, IncrementsBy 1 — c?u h?nh trong AppDbContext).
    ///
    /// Dùng khi:
    ///   - Nhân viên đăng nh?p b?ng m? NV thay v? username.
    ///   - Import Excel ch?a EmployeeCode, c?n lookup EmployeeID tương ?ng.
    ///   - T?m ki?m nhanh trên giao di?n b?ng m? nhân viên.
    /// </summary>
    Task<Employee?> GetByCodeAsync(string employeeCode, bool trackChanges = false, Func<IQueryable<Employee>, IQueryable<Employee>>? include = null, CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra EmployeeCode có b? trùng không — dùng khi t?o m?i ho?c đ?i m?.
    ///
    /// excludeId (int?): lo?i tr? Employee đang đư?c s?a kh?i ki?m tra.
    ///   Ví d?: đang s?a Employee EmployeeID=5 có code "NV1001".
    ///   Không truy?n excludeId ? query th?y chính "NV1001" và báo trùng (sai).
    ///   Truy?n excludeId=5 ? b? qua EmployeeID=5 ? k?t qu? đúng.
    ///
    /// SQL tương đương:
    ///   SELECT TOP 1 1 FROM Employees
    ///   WHERE EmployeeCode = @code AND EmployeeID != @excludeId AND IsDeleted = 0
    /// </summary>
    Task<bool> IsCodeUniqueAsync(
        string code,
        int? excludeId = null,
        CancellationToken ct = default);


    /// <summary>
    /// L?y danh sách c?p dư?i tr?c ti?p c?a m?t qu?n l?.
    ///
    /// D?a trên Employee.SupervisorID (self-reference):
    ///   Employee A có SupervisorID = managerId ? A là c?p dư?i tr?c ti?p.
    ///
    /// Dùng khi:
    ///   - Manager xem danh sách nhân viên m?nh qu?n l? tr?c ti?p.
    ///   - Duy?t LeaveRequest: manager ch? đư?c duy?t c?a subordinates.
    ///   - Assign WorkTask: ch? giao cho ngư?i trong team m?nh qu?n l?.
    ///
    /// Lưu ?: ch? l?y c?p dư?i TR?C TI?P (1 level).
    /// N?u c?n toàn b? cây t? ch?c ? dùng Department.OrgNode (HierarchyId).
    /// </summary>
    Task<IEnumerable<Employee>> GetSubordinatesAsync(int managerId, bool trackChanges = false, Func<IQueryable<Employee>, IQueryable<Employee>>? include = null, CancellationToken ct = default);

    Task<Employee?> GetDetailByIdAsync(int id, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(
    string email,
    int? excludeId = null,
    CancellationToken ct = default);

    Task<bool> IsPhoneUniqueAsync(
        string phone,
        int? excludeId = null,
        CancellationToken ct = default);

    Task<IEnumerable<Employee>> GetManagersAsync(
    bool trackChanges = false,
    Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
    CancellationToken ct = default);
    Task<List<Employee>> GetByDepartmentIdsAsync(
    IEnumerable<int> departmentIds,
    bool trackChanges = false,
    Func<IQueryable<Employee>, IQueryable<Employee>>? include = null,
    CancellationToken ct = default);

    // ── Pre-delete FK existence checks ───────────────────────────────────────
    /// <summary>Trả về true nếu nhân viên có ít nhất 1 bản ghi LeaveRequest.</summary>
    Task<bool> HasLeaveRequestsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Trả về true nếu nhân viên có ít nhất 1 bản ghi AttendanceRecord.</summary>
    Task<bool> HasAttendanceRecordsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Trả về true nếu nhân viên có ít nhất 1 bản ghi PayrollDetail.</summary>
    Task<bool> HasPayrollDetailsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Trả về true nếu nhân viên có ít nhất 1 WorkTask được giao.</summary>
    Task<bool> HasWorkTasksAsync(int employeeId, CancellationToken ct = default);

    Task<List<Employee>> GetByStatusAsync(
    EmployeeStatus status,
    bool trackChanges = false,
    CancellationToken ct = default);

    Task<int> CountByStatusAsync(
        EmployeeStatus status,
        CancellationToken ct = default);

    Task<bool> ExistsByStatusAsync(
        EmployeeStatus status,
        CancellationToken ct = default);

    Task<List<Employee>> GetActiveEmployeesAsync(
    bool trackChanges = false,
    CancellationToken ct = default);
}
