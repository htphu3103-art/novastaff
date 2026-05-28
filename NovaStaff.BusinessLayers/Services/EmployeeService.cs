using Microsoft.EntityFrameworkCore;
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
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using NovaStaff.Shared.Email;
using Microsoft.AspNetCore.Http;

namespace NovaStaff.BusinessLayers.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly IUserRepository _userRepo;          // ← thêm field
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivationTokenService _activationTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;      // ← thêm field
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository deptRepo,
        IUserRepository userRepo,                        // ← thêm
        IUnitOfWork uow,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUser,
        IActivationTokenService activationTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _employeeRepo = employeeRepo;
        _deptRepo = deptRepo;
        _userRepo = userRepo;                            // ← thêm
        _uow = uow;
        _dateTimeService = dateTimeService;
        _currentUser = currentUser;
        _activationTokenService = activationTokenService;
        _emailService = emailService;
        _configuration = configuration;                  // ← thêm
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetFrontendBaseUrl()
    {
        var req = _httpContextAccessor.HttpContext?.Request;
        if (req is not null)
            return $"{req.Scheme}://{req.Host}{req.PathBase}";

        return _configuration["App:FrontendUrl"]!;
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

            // Search
            (string.IsNullOrEmpty(filter.NameContains)
                || e.FullName.Contains(filter.NameContains)) &&

            (string.IsNullOrEmpty(filter.CodeContains)
                || e.EmployeeCode.Contains(filter.CodeContains)) &&

            // Filter
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

            // Permission scope
            (
                !accessibleDepartmentIds.Any()
                || (
                    e.DepartmentID.HasValue &&
                    accessibleDepartmentIds.Contains(
                        e.DepartmentID.Value)
                )
            );

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
            JoinDate = request.JoinDate ?? _dateTimeService.LocalNow.Date,
            Status = EmployeeStatus.Active
        };

        try
        {
            await _employeeRepo.AddAsync(emp, ct);
            await _uow.SaveChangesAsync(ct);
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

        await _userRepo.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        // 7. TẠO ACTIVATION TOKEN → lưu Redis
        var tokenData = new ActivationTokenData(
            UserId: user.UserID,
            Email: emp.Email,
            FullName: emp.FullName
        );

        var token = await _activationTokenService.CreateAsync(tokenData, ct);

        // 8. GỬI EMAIL
        var frontendUrl = GetFrontendBaseUrl();
        var activationLink = $"{frontendUrl}/activate?token={token}";

        _ = _emailService.SendAsync(
            EmployeeEmailTemplates.Welcome(emp.Email, emp.FullName, activationLink),
            ct
        );

        return MapToDto(emp);
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
        var emp = await _employeeRepo.GetByIdAsync(id, true, ct: ct);

        if (emp == null)
            throw new KeyNotFoundException("Nhân viên không tồn tại");

        var hasSub = (await _employeeRepo
            .GetSubordinatesAsync(id, ct: ct))
            .Any();

        if (hasSub)
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang quản lý người khác");

        var managedDepartments = await _deptRepo
    .GetManagedDepartmentsAsync(id, ct);

        if (managedDepartments.Any())
        {
            throw new InvalidOperationException(
                "Không thể xóa nhân viên đang là quản lý phòng ban");
        }

        var user = await _userRepo.GetByEmployeeIdAsync(emp.EmployeeID, ct);

        if (user != null)
        {
            _userRepo.Delete(user);
        }

        _employeeRepo.Delete(emp);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // SQL Server/Postgres messages vary; include outer + inner text
            // so we can reliably match constraint/column names.
            var msg = string.Join(" | ",
                ex.Message ?? "",
                ex.InnerException?.Message ?? "");

            // Nếu vẫn còn FK reference (race condition / dữ liệu không đồng bộ),
            // trả về thông báo nghiệp vụ thay vì message EF generic.
            if (msg.Contains("ManagerEmployeeID", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("FK_Departments_Employees_ManagerEmployeeID", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("IX_Departments_ManagerEmployeeID", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Không thể xóa nhân viên đang là quản lý phòng ban");

            if (msg.Contains("SupervisorID", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("FK_Employees_Employees_SupervisorID", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("IX_Employees_SupervisorID", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Không thể xóa nhân viên đang quản lý người khác");

            throw;
        }
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