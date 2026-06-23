using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Employees;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;
using NovaStaff.Shared.Activation;
using NovaStaff.Shared.Email;
using System.Linq.Expressions;
using NovaStaff.BusinessLayers.Interfaces.Redis;
namespace NovaStaff.BusinessLayers.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivationTokenService _activationTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenBlacklistService _tokenBlacklistService; // 👈 thêm
    private readonly JwtSettings _jwtSettings;                      // 👈 thêm

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository deptRepo,
        IUserRepository userRepo,
        IUnitOfWork uow,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUser,
        IActivationTokenService activationTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokenRepo,
        ILogger<EmployeeService> logger,
        ITokenBlacklistService tokenBlacklistService,   // 👈 thêm
        IOptions<JwtSettings> jwtSettings)              // 👈 thêm
    {
        _employeeRepo = employeeRepo;
        _deptRepo = deptRepo;
        _userRepo = userRepo;
        _uow = uow;
        _dateTimeService = dateTimeService;
        _currentUser = currentUser;
        _activationTokenService = activationTokenService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenRepo = refreshTokenRepo;
        _tokenBlacklistService = tokenBlacklistService; // 👈 thêm
        _jwtSettings = jwtSettings.Value;               // 👈 thêm
    }

    private string GetFrontendBaseUrl()
    {
        var url = _configuration["App:FrontendUrl"];
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("App:FrontendUrl chưa được cấu hình.");
        return url.TrimEnd('/');
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.EmployeeID,
        EmployeeCode = e.EmployeeCode,
        FullName = e.FullName,
        Gender = e.Gender.ToString(),
        BirthDate = e.BirthDate,
        Email = e.Email,
        Phone = e.Phone,
        Address = e.Address,
        Position = e.Position,
        JobLevel = e.JobLevel,
        BaseSalary = e.BaseSalary,
        JoinDate = e.JoinDate,
        ContractType = e.ContractType,
        Status = e.Status.ToString(),
        DepartmentId = e.DepartmentID,
        DepartmentName = e.Department?.DepartmentName,
        SupervisorId = e.SupervisorID,
        SupervisorName = e.Supervisor?.FullName,
    };

    // ========================= READ =========================

    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var emp = await _employeeRepo.GetDetailByIdAsync(id, ct);
        if (emp == null)
            throw new KeyNotFoundException($"Không tìm thấy nhân viên ID {id}");
        return MapToDto(emp);
    }

    public async Task<EmployeeDto> GetByCodeAsync(string employeeCode, CancellationToken ct = default)
    {
        var emp = await _employeeRepo.GetByCodeAsync(
            employeeCode,
            trackChanges: false,
            include: q => q.Include(x => x.Department).Include(x => x.Supervisor),
            ct: ct);

        if (emp == null)
            throw new KeyNotFoundException($"Không tìm thấy nhân viên mã {employeeCode}");

        return MapToDto(emp);
    }

    public async Task<PagedResult<EmployeeDto>> GetPagedAsync(
    EmployeeFilter filter,
    int pageIndex,
    int pageSize,
    CancellationToken ct = default)
    {
        // Department mà current user được phép xem
        var accessibleDepartmentIds =
            await GetAccessibleDepartmentIdsAsync(ct);

        Expression<Func<Employee, bool>> predicate = e =>

    (string.IsNullOrEmpty(filter.NameContains)
        || e.FullName.Contains(filter.NameContains)) &&

    (string.IsNullOrEmpty(filter.CodeContains)
        || e.EmployeeCode.Contains(filter.CodeContains)) &&

    (!filter.DepartmentId.HasValue
        || e.DepartmentID == filter.DepartmentId) &&

    (!filter.SupervisorId.HasValue
        || e.SupervisorID == filter.SupervisorId) &&

    (!filter.Status.HasValue
        || e.Status == filter.Status) &&

    (!filter.Gender.HasValue
        || e.Gender == filter.Gender) &&

    (string.IsNullOrEmpty(filter.ContractType)
        || e.ContractType == filter.ContractType) &&

    // ── Thêm 2 dòng này ──────────────────────────────────
    (!filter.JoinDateFrom.HasValue
        || e.JoinDate >= filter.JoinDateFrom.Value) &&

    (!filter.JoinDateTo.HasValue
        || e.JoinDate <= filter.JoinDateTo.Value) &&
    // ─────────────────────────────────────────────────────

    (!accessibleDepartmentIds.Any()
        || (
            e.DepartmentID.HasValue &&
            accessibleDepartmentIds.Contains(e.DepartmentID.Value)
        ));

        Func<IQueryable<Employee>,
            IOrderedQueryable<Employee>> orderBy =
            filter.SortBy switch
            {
                EmployeeSortField.EmployeeCode =>
                    q => filter.SortDescending
                        ? q.OrderByDescending(x => x.EmployeeCode)
                        : q.OrderBy(x => x.EmployeeCode),

                EmployeeSortField.JoinDate =>
                    q => filter.SortDescending
                        ? q.OrderByDescending(x => x.JoinDate)
                        : q.OrderBy(x => x.JoinDate),

                EmployeeSortField.BaseSalary =>
                    q => filter.SortDescending
                        ? q.OrderByDescending(x => x.BaseSalary)
                        : q.OrderBy(x => x.BaseSalary),

                EmployeeSortField.Department =>
                    q => filter.SortDescending
                        ? q.OrderByDescending(x => x.Department!.DepartmentName)
                        : q.OrderBy(x => x.Department!.DepartmentName),

                _ =>
                    q => filter.SortDescending
                        ? q.OrderByDescending(x => x.FullName)
                        : q.OrderBy(x => x.FullName),
            };

        var result = await _employeeRepo.GetPagedAsync(
            pageIndex,
            pageSize,
            filter: predicate,
            orderBy: orderBy,
            include: q => q
                .Include(x => x.Department)
                .Include(x => x.Supervisor),
            trackChanges: false,
            ct: ct);

        return new PagedResult<EmployeeDto>(
            result.Items.Select(MapToDto).ToList(),
            result.TotalCount,
            result.PageIndex,
            result.PageSize);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default)
    {
        if (!await _deptRepo.ExistsAsync(departmentId, ct))
            throw new KeyNotFoundException($"Không tìm thấy phòng ban ID {departmentId}");

        var result = await _employeeRepo.GetPagedAsync(1, int.MaxValue, e => e.DepartmentID == departmentId,
            q => q.OrderBy(e => e.FullName), q => q.Include(x => x.Department).Include(x => x.Supervisor), false, ct);

        return result.Items.Select(MapToDto);
    }

    public async Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(int managerId, CancellationToken ct = default)
    {
        if (!await _employeeRepo.ExistsAsync(managerId, ct))
            throw new KeyNotFoundException($"Không tìm thấy quản lý ID {managerId}");

        var emps = await _employeeRepo.GetSubordinatesAsync(managerId, false, q => q.Include(x => x.Department).Include(x => x.Supervisor), ct);
        return emps.Select(MapToDto);
    }
    public async Task<IEnumerable<EmployeeManagerDto>> GetManagersAsync(CancellationToken ct = default)
    {
        var result = await _employeeRepo.GetPagedAsync(
            pageIndex: 1,
            pageSize: int.MaxValue,
            filter: e => e.User != null && e.User.Role == UserRole.Manager,
            orderBy: q => q.OrderBy(e => e.FullName),
            include: q => q.Include(e => e.Department).Include(e => e.User),
            trackChanges: false,
            ct: ct);

        return result.Items.Select(e => new EmployeeManagerDto
        {
            EmployeeID = e.EmployeeID,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Position = e.Position,
            DepartmentId = e.DepartmentID,
            DepartmentName = e.Department?.DepartmentName,
            Email = e.Email,
            Phone = e.Phone,
        });
    }
    // ========================= CREATE =========================

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        // 1. VALIDATION CƠ BẢN
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Tên không được rỗng");

        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
            throw new ArgumentException("Mã NV không được rỗng");

        // 2. NORMALIZE
        var code = request.EmployeeCode.Trim().ToUpperInvariant();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone?.Trim();

        // 3. UNIQUE CHECK
        if (!await _employeeRepo.IsCodeUniqueAsync(code, null, ct))
            throw new InvalidOperationException($"Mã NV {code} đã tồn tại");

        if (!await _employeeRepo.IsEmailUniqueAsync(email, null, ct))
            throw new InvalidOperationException("Email đã tồn tại");

        if (!string.IsNullOrWhiteSpace(phone) && !await _employeeRepo.IsPhoneUniqueAsync(phone, null, ct))
            throw new InvalidOperationException("Số điện thoại đã tồn tại");

        // 4. FK VALIDATION
        if (request.DepartmentId.HasValue && !await _deptRepo.ExistsAsync(request.DepartmentId.Value, ct))
            throw new KeyNotFoundException("Phòng ban không tồn tại");

        if (request.SupervisorId.HasValue && !await _employeeRepo.ExistsAsync(request.SupervisorId.Value, ct))
            throw new KeyNotFoundException("Quản lý không tồn tại");

        return await _uow.ExecuteInTransactionAsync(async (transactionCt) =>
        {
            // 5. MAP DỮ LIỆU VÀO ENTITY
            var emp = new Employee
            {
                EmployeeCode = code,
                FullName = request.FullName,
                Email = email,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                Phone = phone,
                Address = request.Address,
                DepartmentID = request.DepartmentId,
                SupervisorID = request.SupervisorId,
                Position = request.Position,
                JobLevel = request.JobLevel,
                BaseSalary = request.BaseSalary,
                ContractType = request.ContractType,
                JoinDate = request.JoinDate
                            ?? DateOnly.FromDateTime(_dateTimeService.UtcNow.UtcDateTime),
                Status = EmployeeStatus.Active
            };

            try
            {
                await _employeeRepo.AddAsync(emp, transactionCt);
                await _uow.SaveChangesAsync(transactionCt);
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? "";

                if (msg.Contains("IX_Employees_Email"))
                    throw new InvalidOperationException("Email đã tồn tại");

                if (msg.Contains("IX_Employees_EmployeeCode"))
                    throw new InvalidOperationException("Mã NV đã tồn tại");

                if (msg.Contains("IX_Employees_Phone"))
                    throw new InvalidOperationException("SĐT đã tồn tại");

                throw;
            }

            // 6. TẠO USER
            var user = new User
            {
                EmployeeID = emp.EmployeeID,
                Username = email,
                PasswordHash = string.Empty,
                IsActive = false,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            await _userRepo.AddAsync(user, transactionCt);

            try
            {
                await _uow.SaveChangesAsync(transactionCt);
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? "";

                if (msg.Contains("IX_Users_Username", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("Username", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Email này đã được dùng cho một tài khoản khác (kể cả tài khoản đã bị xóa).");

                if (msg.Contains("IX_Users_EmployeeID", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("EmployeeID", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Nhân viên này đã có tài khoản trong hệ thống.");

                throw;
            }

            // 7. TẠO ACTIVATION TOKEN → lưu Redis
            var tokenData = new ActivationTokenData(
                UserId: user.UserID,
                Email: emp.Email,
                FullName: emp.FullName
            );

            var token = await _activationTokenService.CreateAsync(tokenData, transactionCt);

            // 8. GỬI EMAIL
            var frontendUrl = GetFrontendBaseUrl();
            var activationLink = $"{frontendUrl}/activate?token={token}";

            _logger.LogInformation("Đang gửi email kích hoạt cho {Email}", emp.Email);

            try
            {
                await _emailService.SendAsync(
                    EmployeeEmailTemplates.Welcome(emp.Email, emp.FullName, activationLink),
                    CancellationToken.None
                );
                _logger.LogInformation("Gửi email thành công cho {Email}", emp.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email thất bại cho {Email}. Tiến hành rollback giao dịch.", emp.Email);
                throw new InvalidOperationException($"Không thể gửi email kích hoạt. Lỗi: {ex.Message}", ex);
            }

            return MapToDto(emp);
        }, System.Data.IsolationLevel.ReadCommitted, ct);
    }

    // ========================= UPDATE =========================

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var emp = await _employeeRepo.GetByIdAsync(id, true, ct: ct);
        if (emp == null)
            throw new KeyNotFoundException("Nhân viên không tồn tại");

        // 1. Lưu lại dữ liệu cũ trước khi ghi đè (Dùng cho Audit)
        var oldData = new
        {
            emp.FullName,
            emp.Email,
            emp.Phone,
            emp.Position,
            emp.DepartmentID,
            emp.SupervisorID,
            emp.Status,
            emp.BaseSalary
        };

        // 2. NORMALIZE (Chuẩn hóa dữ liệu đầu vào)
        var code = request.EmployeeCode.Trim().ToUpperInvariant();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone?.Trim();

        // 3. UNIQUE CHECK (Kiểm tra trùng lặp)
        if (!await _employeeRepo.IsCodeUniqueAsync(code, id, ct))
            throw new InvalidOperationException("Mã NV đã tồn tại");

        if (!await _employeeRepo.IsEmailUniqueAsync(email, id, ct))
            throw new InvalidOperationException("Email đã tồn tại");

        if (!string.IsNullOrWhiteSpace(phone) && !await _employeeRepo.IsPhoneUniqueAsync(phone, id, ct))
            throw new InvalidOperationException("Số điện thoại đã tồn tại");

        // 4. FK VALIDATION (Kiểm tra khóa ngoại)
        if (request.DepartmentId.HasValue && !await _deptRepo.ExistsAsync(request.DepartmentId.Value, ct))
            throw new KeyNotFoundException("Phòng ban không tồn tại");

        if (request.SupervisorId.HasValue)
        {
            if (request.SupervisorId.Value == id)
                throw new InvalidOperationException("Không thể tự làm quản lý");

            if (!await _employeeRepo.ExistsAsync(request.SupervisorId.Value, ct))
                throw new KeyNotFoundException("Quản lý không tồn tại");

            if (await IsCircularReferenceAsync(id, request.SupervisorId.Value, ct))
                throw new InvalidOperationException("Tạo vòng lặp quản lý");
        }

        // 5. UPDATE DATA (Cập nhật dữ liệu vào Entity)
        emp.EmployeeCode = code;
        emp.FullName = request.FullName;
        emp.Email = email;
        emp.Gender = request.Gender;
        emp.BirthDate = request.BirthDate;
        emp.Phone = phone;
        emp.Address = request.Address;
        emp.DepartmentID = request.DepartmentId;
        emp.SupervisorID = request.SupervisorId;
        emp.Position = request.Position;
        emp.JobLevel = request.JobLevel;
        emp.BaseSalary = request.BaseSalary;
        emp.ContractType = request.ContractType;
        emp.Status = request.Status;
        emp.JoinDate = request.JoinDate ?? emp.JoinDate;

        try
        {
            // 6. GỌI REPOSITORY (Đánh dấu trạng thái Modified)
            _employeeRepo.Update(emp);
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // 9. CATCH LOGIC (Xử lý lỗi cấp độ Database)
            var msg = ex.InnerException?.Message ?? "";

            if (msg.Contains("IX_Employees_Email"))
                throw new InvalidOperationException("Email đã tồn tại");

            if (msg.Contains("IX_Employees_EmployeeCode"))
                throw new InvalidOperationException("Mã NV đã tồn tại");

            if (msg.Contains("IX_Employees_Phone"))
                throw new InvalidOperationException("SĐT đã tồn tại");

            throw; // Nếu là lỗi khác, ném ra ngoài
        }

        return MapToDto(emp);
    }

    // ========================= DELETE =========================

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var emp = await _employeeRepo.GetByIdAsync(
            id,
            trackChanges: true,
            ct: ct);

        if (emp == null)
            throw new KeyNotFoundException("Nhân viên không tồn tại.");

        // =====================================================
        // Chỉ cho phép xóa nhân viên đang hoạt động / thử việc
        // =====================================================

        if (emp.Status != EmployeeStatus.Active &&
            emp.Status != EmployeeStatus.Probation)
        {
            throw new InvalidOperationException(
                "Chỉ được xóa nhân viên đang hoạt động hoặc thử việc.");
        }

        // =====================================================
        // Không được quản lý nhân viên khác
        // =====================================================

        var hasSubordinates = (await _employeeRepo
            .GetSubordinatesAsync(id, ct: ct))
            .Any();

        if (hasSubordinates)
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang quản lý người khác.");
        }

        // =====================================================
        // Không được là trưởng phòng
        // =====================================================

        var managedDepartments = await _deptRepo
            .GetManagedDepartmentsAsync(id, ct);

        if (managedDepartments.Any())
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang là quản lý phòng ban.");
        }

        // =====================================================
        // Không được có dữ liệu nghiệp vụ
        // =====================================================

        if (await _employeeRepo.HasLeaveRequestsAsync(emp.EmployeeID, ct))
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang có dữ liệu nghỉ phép.");
        }

        if (await _employeeRepo.HasAttendanceRecordsAsync(emp.EmployeeID, ct))
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang có dữ liệu chấm công.");
        }

        if (await _employeeRepo.HasPayrollDetailsAsync(emp.EmployeeID, ct))
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang có dữ liệu lương.");
        }

        if (await _employeeRepo.HasWorkTasksAsync(emp.EmployeeID, ct))
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang có công việc được giao.");
        }

        // =====================================================
        // Không được có tài khoản hệ thống
        // =====================================================

        var user = await _userRepo.GetByEmployeeIdAsync(
            emp.EmployeeID,
            trackChanges: false,
            ct: ct);

        if (user != null)
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đã có tài khoản người dùng.");
        }

        // =====================================================
        // Hard Delete
        // =====================================================

        _employeeRepo.Delete(emp);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var msg = string.Join(" | ",
                ex.Message ?? string.Empty,
                ex.InnerException?.Message ?? string.Empty);

            if (msg.Contains("ManagerEmployeeID",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang là quản lý phòng ban.");
            }

            if (msg.Contains("SupervisorID",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang quản lý người khác.");
            }

            if (msg.Contains("LeaveRequest",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang có dữ liệu nghỉ phép.");
            }

            if (msg.Contains("AttendanceRecord",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang có dữ liệu chấm công.");
            }

            if (msg.Contains("PayrollDetail",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang có dữ liệu lương.");
            }

            if (msg.Contains("WorkTask",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể xóa nhân viên đang có công việc được giao.");
            }

            throw;
        }
    }

    public async Task ChangeStatusAsync(
    int employeeId,
    EmployeeStatus status,
    CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(EmployeeStatus), status))
            throw new ArgumentException("Trạng thái không hợp lệ.");

        var employee = await _employeeRepo.GetByIdAsync(
            employeeId,
            trackChanges: true,
            ct: ct);

        if (employee == null)
            throw new KeyNotFoundException("Nhân viên không tồn tại.");

        if (employee.Status == status)
            return;

        await _uow.ExecuteInTransactionAsync(async ct =>
        {
            employee.Status = status;

            var user = await _userRepo.GetByEmployeeIdAsync(
                employeeId,
                trackChanges: true,
                ct: ct);

            if (user != null)
            {
                bool shouldLock = status is EmployeeStatus.Resigned
                                         or EmployeeStatus.Terminated
                                         or EmployeeStatus.Retired
                                         or EmployeeStatus.Deceased;

                bool shouldUnlock = status is EmployeeStatus.Active
                                           or EmployeeStatus.Probation
                                           or EmployeeStatus.OnLeave;

                if (shouldLock)
                {
                    employee.TerminationDate = DateOnly.FromDateTime(DateTime.Today);
                    await _userRepo.LockAsync(user.UserID, ct);
                    await _refreshTokenRepo.RevokeAllByUserAsync(user.UserID, ct);
                    await _tokenBlacklistService.BlacklistUserAsync(  // 👈 thêm
                        user.UserID,
                        TimeSpan.FromMinutes(_jwtSettings.AccessTokenMinutes),
                        ct);
                }
                else if (shouldUnlock)
                {
                    employee.TerminationDate = null;
                    await _userRepo.UnlockAsync(user.UserID, ct);
                }
            }

            await _uow.SaveChangesAsync(ct);
            return true; // ExecuteInTransactionAsync cần TResult
        }, ct: ct);
    }

    // ========================= DOMAIN LOGIC =========================

    public async Task TransferDepartmentAsync(int id, int newDepartmentId, CancellationToken ct = default)
    {
        var emp = await _employeeRepo.GetByIdAsync(id, true, ct: ct);
        if (emp == null) throw new KeyNotFoundException("Nhân viên không tồn tại");

        if (!await _deptRepo.ExistsAsync(newDepartmentId, ct))
            throw new KeyNotFoundException("Phòng ban đích không tồn tại");

        if (emp.DepartmentID == newDepartmentId)
            throw new InvalidOperationException("Nhân viên đã ở trong phòng ban này");

        int? oldDeptId = emp.DepartmentID;
        emp.DepartmentID = newDepartmentId;

        _employeeRepo.Update(emp);

        await _uow.SaveChangesAsync(ct);
    }

    private async Task<bool> IsCircularReferenceAsync(int employeeId, int newSupervisorId, CancellationToken ct)
    {
        int? currentSupervisorId = newSupervisorId;
        while (currentSupervisorId.HasValue)
        {
            if (currentSupervisorId.Value == employeeId) return true;
            var supervisor = await _employeeRepo.GetByIdAsync(currentSupervisorId.Value, false, null, ct);
            if (supervisor == null) break;
            currentSupervisorId = supervisor.SupervisorID;
        }
        return false;
    }

    private async Task<List<int>> GetAccessibleDepartmentIdsAsync(
    CancellationToken ct)
    {
        var role = _currentUser.GetRole();

        // HR/Admin => unrestricted
        if (role != UserRole.Manager.ToString())
            return [];

        var employeeId = _currentUser.GetEmployeeId()
            ?? throw new UnauthorizedAccessException(
                "Không xác định được nhân viên.");

        var managedDepartments = await _deptRepo
            .GetManagedDepartmentsAsync(employeeId, ct);

        var rootIds = managedDepartments
            .Select(x => x.DepartmentID)
            .ToList();

        var descendantIds = await _deptRepo
            .GetDescendantIdsAsync(rootIds, ct);

        return rootIds
            .Union(descendantIds)
            .Distinct()
            .ToList();
    }
}