// Services/Interfaces/IEmployeeService.cs
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Employees;
using NovaStaff.Models.Filters;

namespace NovaStaff.Services.Interfaces;

/// <summary>
/// Service layer cho Employee — entity trung tâm của hệ thống NovaStaff.
///
/// Phân công trách nhiệm:
///   IEmployeeService  → business logic, validation, transaction, audit
///   IEmployeeRepository → data access, query, persistence
///
/// Mọi exception từ service sẽ được GlobalExceptionMiddleware bắt và map sang HTTP code:
///   KeyNotFoundException       → 404 Not Found
///   ArgumentException          → 400 Bad Request
///   InvalidOperationException  → 409 Conflict / 422 Unprocessable
/// </summary>
public interface IEmployeeService
{
    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Lấy thông tin nhân viên theo ID nội bộ (PK của DB).
    ///
    /// Dùng khi:
    ///   - Load trang chi tiết nhân viên (Employee Detail Page).
    ///   - Kiểm tra tồn tại trước khi thực hiện thao tác nghiệp vụ.
    ///   - Các service khác (LeaveRequest, Payroll) cần thông tin cơ bản của nhân viên.
    ///
    /// Throws:
    ///   KeyNotFoundException nếu không tìm thấy.
    /// </summary>
    Task<EmployeeDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Lấy thông tin nhân viên theo EmployeeCode (mã nghiệp vụ, ví dụ "NV1001").
    ///
    /// EmployeeCode khác EmployeeID:
    ///   EmployeeID   : khoá chính DB, do PostgreSQL tự sinh (int identity).
    ///   EmployeeCode : mã nghiệp vụ, do sequence "EmployeeCodeSequence" sinh
    ///                  (StartsAt 1000, IncrementsBy 1).
    ///
    /// Dùng khi:
    ///   - Import Excel chứa EmployeeCode, cần lookup EmployeeID tương ứng.
    ///   - Tìm kiếm nhanh trên giao diện bằng mã nhân viên.
    ///   - Nhân viên đăng nhập bằng mã NV thay vì username.
    ///
    /// Throws:
    ///   ArgumentException    nếu code rỗng.
    ///   KeyNotFoundException nếu không tìm thấy.
    /// </summary>
    Task<EmployeeDto> GetByCodeAsync(string code, CancellationToken ct = default);


    /// <summary>
    /// Lấy danh sách nhân viên có phân trang và bộ lọc.
    ///
    /// Hỗ trợ lọc theo:
    ///   NameContains  : tìm kiếm theo tên (LIKE %name%).
    ///   DepartmentId  : lọc theo phòng ban.
    ///   IsActive      : lọc theo trạng thái hoạt động.
    ///   SortBy        : sắp xếp theo field chỉ định.
    ///   SortDescending: chiều sắp xếp.
    ///
    /// Dùng khi:
    ///   - Màn hình danh sách nhân viên (Employee List Page) với search + filter + paging.
    ///   - Export báo cáo nhân sự theo điều kiện.
    ///
    /// Không throw nếu không có kết quả — trả về PagedResult với Items rỗng.
    /// </summary>
    Task<PagedResult<EmployeeDto>> GetPagedAsync(
        EmployeeFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy tất cả nhân viên thuộc một phòng ban (không phân trang).
    ///
    /// Dùng khi:
    ///   - Hiển thị danh sách nhân viên trong màn hình quản lý phòng ban.
    ///   - Tính lương hàng loạt cho một phòng ban (loop để tạo PayrollDetail).
    ///   - Dropdown chọn nhân viên trong phòng khi tạo WorkTask.
    ///
    /// Lưu ý: Employee.DepartmentID là nullable — nhân viên mới có thể
    /// chưa được phân phòng ban (sẽ không xuất hiện trong kết quả).
    ///
    /// Throws:
    ///   KeyNotFoundException nếu phòng ban không tồn tại.
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(
        int departmentId,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách cấp dưới trực tiếp của một quản lý.
    ///
    /// Dựa trên Employee.SupervisorID (self-reference):
    ///   Employee A có SupervisorID = managerId → A là cấp dưới trực tiếp.
    ///
    /// CHỈ lấy cấp dưới TRỰC TIẾP (1 level).
    /// Nếu cần toàn bộ cây tổ chức → dùng Department.OrgNode (HierarchyId).
    ///
    /// Dùng khi:
    ///   - Manager xem danh sách nhân viên mình quản lý trực tiếp.
    ///   - Duyệt LeaveRequest: manager chỉ được duyệt của subordinates.
    ///   - Assign WorkTask: chỉ giao cho người trong team mình quản lý.
    ///   - Validate trước khi xóa nhân viên (còn subordinates → chặn xóa).
    ///
    /// Throws:
    ///   KeyNotFoundException nếu manager không tồn tại.
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(
        int managerId,
        CancellationToken ct = default);

    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Tạo mới nhân viên.
    ///
    /// Validation trước khi tạo:
    ///   - FullName không được rỗng.
    ///   - EmployeeCode không được rỗng và phải unique toàn hệ thống.
    ///   - DepartmentId (nếu có) phải tồn tại.
    ///   - SupervisorId (nếu có) phải là nhân viên đang tồn tại.
    ///
    /// Side effects:
    ///   - Ghi AuditLog với action Insert.
    ///   - IsActive mặc định = true.
    ///   - HireDate mặc định = ngày hôm nay nếu không truyền.
    ///   - EmployeeCode được normalize: Trim + ToUpperInvariant.
    ///
    /// Throws:
    ///   ArgumentException        nếu FullName hoặc EmployeeCode rỗng.
    ///   InvalidOperationException nếu EmployeeCode đã tồn tại.
    ///   KeyNotFoundException      nếu DepartmentId hoặc SupervisorId không tồn tại.
    /// </summary>
    Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken ct = default);

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Cập nhật thông tin nhân viên.
    ///
    /// Validation trước khi cập nhật:
    ///   - FullName không được rỗng.
    ///   - EmployeeCode phải unique (bỏ qua chính nhân viên đang sửa).
    ///   - DepartmentId (nếu có) phải tồn tại.
    ///   - SupervisorId (nếu có) phải tồn tại và KHÔNG được là chính nhân viên đó
    ///     (tránh circular reference trong cây supervisor).
    ///
    /// Side effects:
    ///   - Ghi AuditLog với action Update, lưu snapshot trạng thái cũ (oldValue).
    ///
    /// Lưu ý: KHÔNG dùng hàm này để thuyên chuyển phòng ban chính thức —
    /// dùng TransferDepartmentAsync để có audit trail độc lập cho sự kiện đó.
    ///
    /// Throws:
    ///   KeyNotFoundException      nếu nhân viên, phòng ban hoặc supervisor không tồn tại.
    ///   ArgumentException         nếu FullName hoặc EmployeeCode rỗng.
    ///   InvalidOperationException nếu EmployeeCode trùng hoặc SupervisorId là chính mình.
    /// </summary>
    Task<EmployeeDto> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken ct = default);

    // =========================================================
    // DELETE
    // =========================================================

    /// <summary>
    /// Xóa vĩnh viễn nhân viên (Hard Delete).
    ///
    /// Hệ thống dùng Hard Delete — dữ liệu trước khi xóa được
    /// AuditInterceptor chụp lại tự động vào bảng AuditLogs.
    ///
    /// Điều kiện CHẶN xóa (throw InvalidOperationException):
    ///   1. Nhân viên đang là SupervisorID của nhân viên khác
    ///      → Yêu cầu đổi supervisor cho cấp dưới trước.
    ///   2. Nhân viên đang là ManagerEmployeeID của phòng ban
    ///      → Yêu cầu thay trưởng phòng trước.
    ///
    /// Side effects:
    ///   - Ghi AuditLog với action Delete TRƯỚC khi xóa (lưu snapshot).
    ///   - Xóa vật lý khỏi DB sau khi audit xong.
    ///
    /// Throws:
    ///   KeyNotFoundException      nếu nhân viên không tồn tại.
    ///   InvalidOperationException nếu vi phạm điều kiện chặn xóa ở trên.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    // =========================================================
    // DOMAIN OPERATIONS
    // =========================================================

    /// <summary>
    /// Thuyên chuyển nhân viên sang phòng ban khác.
    ///
    /// Tách riêng khỏi UpdateAsync vì đây là sự kiện nghiệp vụ độc lập,
    /// cần audit trail rõ ràng: ai chuyển, từ phòng nào, sang phòng nào, lúc nào.
    /// Dễ extend sau: trigger thông báo email, lưu lịch sử transfer, v.v.
    ///
    /// Validation:
    ///   - Phòng ban đích phải tồn tại.
    ///   - Phòng ban đích phải khác phòng ban hiện tại
    ///     (tránh ghi audit log vô nghĩa).
    ///
    /// Side effects:
    ///   - Cập nhật Employee.DepartmentID.
    ///   - Ghi AuditLog với action Update, ghi rõ OldDepartmentId → NewDepartmentId.
    ///
    /// Throws:
    ///   KeyNotFoundException      nếu nhân viên hoặc phòng ban đích không tồn tại.
    ///   InvalidOperationException nếu phòng ban đích trùng phòng ban hiện tại.
    /// </summary>
    Task TransferDepartmentAsync(
        int id,
        int newDepartmentId,
        CancellationToken ct = default);

    Task<IEnumerable<EmployeeManagerDto>> GetManagersAsync(CancellationToken ct = default);
}