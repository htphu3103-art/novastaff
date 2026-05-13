// BusinessLayers/Services/AttendanceService.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Attendance;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;
using System.Linq.Expressions;

namespace NovaStaff.BusinessLayers.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepo;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IUserRepository userRepo,
        IUnitOfWork uow,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUser)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _userRepo = userRepo;
        _uow = uow;
        _dateTimeService = dateTimeService;
        _currentUser = currentUser;
    }
    private DateTime GetLocalNow() =>
    _dateTimeService.UtcNow.AddHours(7);
    // =========================================================
    // MAPPER
    // =========================================================

    private static AttendanceDto MapToDto(AttendanceRecord r) => new()
    {
        RecordId = r.RecordID,
        EmployeeId = r.EmployeeID,
        EmployeeCode = r.Employee?.EmployeeCode,
        EmployeeName = r.Employee?.FullName,
        WorkDate = r.WorkDate,
        CheckIn = r.CheckIn,
        CheckOut = r.CheckOut,
        WorkHours = r.WorkHours,
        Status = r.Status,
    };

    // =========================================================
    // READ
    // =========================================================

    public async Task<AttendanceDto?> GetTodayAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, ct);

        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        return record == null ? null : MapToDto(record);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByEmployeeAndMonthAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        ValidateYearMonth(year, month);
        await EnsureEmployeeExistsAsync(employeeId, ct);

        var records = await _attendanceRepo.GetByEmployeeAndMonthAsync(employeeId, year, month, ct);
        return records.Select(MapToDto);
    }

    public async Task<PagedResult<AttendanceDto>> GetPagedAsync(
        AttendanceFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        // Validate date range
        if (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

        Expression<Func<AttendanceRecord, bool>> predicate = r =>
            (!filter.EmployeeId.HasValue || r.EmployeeID == filter.EmployeeId) &&
            (!filter.DepartmentId.HasValue || r.Employee!.DepartmentID == filter.DepartmentId) &&
            (!filter.From.HasValue || r.WorkDate >= filter.From) &&
            (!filter.To.HasValue || r.WorkDate <= filter.To) &&
            (!filter.Status.HasValue || r.Status == filter.Status);

        Func<IQueryable<AttendanceRecord>, IOrderedQueryable<AttendanceRecord>> orderBy = filter.SortDescending
            ? q => q.OrderByDescending(r => r.WorkDate).ThenBy(r => r.EmployeeID)
            : q => q.OrderBy(r => r.WorkDate).ThenBy(r => r.EmployeeID);

        var result = await _attendanceRepo.GetPagedAsync(
            pageIndex, pageSize,
            filter: predicate,
            orderBy: orderBy,
            include: q => q.Include(r => r.Employee),
            trackChanges: false,
            ct: ct);

        return new PagedResult<AttendanceDto>(
            result.Items.Select(MapToDto).ToList(),
            result.TotalCount, result.PageIndex, result.PageSize);
    }

    public async Task<double> GetTotalHoursAsync(
        int employeeId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        ValidateYearMonth(year, month);
        await EnsureEmployeeExistsAsync(employeeId, ct);

        return await _attendanceRepo.GetTotalHoursAsync(employeeId, year, month, ct);
    }
    public async Task<double> GetMyTotalHoursAsync(
    int year,
    int month,
    CancellationToken ct = default)
    {
        ValidateYearMonth(year, month);

        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        return await _attendanceRepo.GetTotalHoursAsync(employeeId, year, month, ct);
    }
    // =========================================================
    // ACTIONS — Check-in / Check-out
    // =========================================================

    public async Task<AttendanceDto> CheckInAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, ct);

        // Chỉ cho phép check-in 1 lần / ngày
        var existing = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (existing != null)
            throw new InvalidOperationException("Nhân viên đã check-in hôm nay");

        var now = GetLocalNow();

        var record = new AttendanceRecord
        {
            EmployeeID = employeeId,
            WorkDate = now.Date,
            CheckIn = now,
            Status = AttendanceStatus.Present,
        };

        await _attendanceRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(record);
    }

    public async Task<AttendanceDto> CheckOutAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, ct);

        // Phải check-in trước
        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (record == null)
            throw new InvalidOperationException("Nhân viên chưa check-in hôm nay");

        // Tránh check-out 2 lần
        if (record.CheckOut.HasValue)
            throw new InvalidOperationException("Nhân viên đã check-out hôm nay");

        // Cần trackChanges=true để EF theo dõi thay đổi
        var tracked = await _attendanceRepo.GetByIdAsync(record.RecordID, trackChanges: true, ct: ct);
        tracked!.CheckOut = GetLocalNow();

        // WorkHours là computed column trong DB, sau SaveChanges EF sẽ reload
        _attendanceRepo.Update(tracked);
        await _uow.SaveChangesAsync(ct);

        // Reload để lấy WorkHours mà DB đã tính
        await _attendanceRepo.ReloadAsync(tracked, ct);

        return MapToDto(tracked);
    }

    // =========================================================
    // HR MANAGEMENT
    // =========================================================

    public async Task<AttendanceDto> CreateManualAsync(
        CreateAttendanceRequest request,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(request.EmployeeId, ct);
        ValidateCheckInOut(request.CheckIn, request.CheckOut);

        // Kiểm tra trùng ngày
        var workDate = request.WorkDate.Date;
        var duplicateExists = await _attendanceRepo.ExistsAsync(
            r => r.EmployeeID == request.EmployeeId && r.WorkDate == workDate, ct);

        if (duplicateExists)
            throw new InvalidOperationException(
                $"Đã tồn tại record chấm công ngày {workDate:dd/MM/yyyy} cho nhân viên này");

        var record = new AttendanceRecord
        {
            EmployeeID = request.EmployeeId,
            WorkDate = workDate,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Status = request.Status,
        };

        await _attendanceRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(record);
    }

    public async Task<AttendanceDto> UpdateAsync(
        long recordId,
        UpdateAttendanceRequest request,
        CancellationToken ct = default)
    {
        var record = await _attendanceRepo.GetByIdAsync(recordId, trackChanges: true, ct: ct);
        if (record == null)
            throw new KeyNotFoundException($"Không tìm thấy record chấm công ID {recordId}");

        ValidateCheckInOut(request.CheckIn, request.CheckOut);

        record.CheckIn = request.CheckIn;
        record.CheckOut = request.CheckOut;
        record.Status = request.Status;

        _attendanceRepo.Update(record);
        await _uow.SaveChangesAsync(ct);

        // Reload để lấy WorkHours computed column
        await _attendanceRepo.ReloadAsync(record, ct);

        return MapToDto(record);
    }

    public async Task DeleteAsync(long recordId, CancellationToken ct = default)
    {
        var record = await _attendanceRepo.GetByIdAsync(recordId, trackChanges: true, ct: ct);
        if (record == null)
            throw new KeyNotFoundException($"Không tìm thấy record chấm công ID {recordId}");

        _attendanceRepo.Delete(record);
        await _uow.SaveChangesAsync(ct);
    }

    // =========================================================
    // PRIVATE HELPERS
    // =========================================================

    private async Task EnsureEmployeeExistsAsync(int employeeId, CancellationToken ct)
    {
        if (!await _employeeRepo.ExistsAsync(employeeId, ct))
            throw new KeyNotFoundException($"Không tìm thấy nhân viên ID {employeeId}");
    }

    private static void ValidateYearMonth(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Năm không hợp lệ");

        if (month < 1 || month > 12)
            throw new ArgumentException("Tháng không hợp lệ (1–12)");
    }
    public async Task<AttendanceDto> CheckInAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var existing = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (existing != null)
            throw new InvalidOperationException("Đã check-in hôm nay");

        var now = GetLocalNow();

        var record = new AttendanceRecord
        {
            EmployeeID = employeeId,
            WorkDate = now.Date,
            CheckIn = now,
            Status = AttendanceStatus.Present,
        };

        await _attendanceRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(record);
    }
    public async Task<AttendanceDto?> GetTodayForCurrentUserAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        return record == null ? null : MapToDto(record);
    }
    private async Task<int> GetCurrentEmployeeIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException("Unauthenticated");

        var employeeId = await _userRepo.GetEmployeeIdByUserIdAsync(userId, ct);

        if (employeeId == null)
            throw new InvalidOperationException("User chưa liên kết Employee");

        return employeeId.Value;
    }

    public async Task<AttendanceDto> CheckOutAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (record == null)
            throw new InvalidOperationException("Chưa check-in");

        if (record.CheckOut.HasValue)
            throw new InvalidOperationException("Đã check-out");

        var tracked = await _attendanceRepo.GetByIdAsync(record.RecordID, true, ct: ct);

        tracked!.CheckOut = GetLocalNow();

        _attendanceRepo.Update(tracked);
        await _uow.SaveChangesAsync(ct);

        await _attendanceRepo.ReloadAsync(tracked, ct);

        return MapToDto(tracked);
    }

    private static void ValidateCheckInOut(DateTime? checkIn, DateTime? checkOut)
    {
        // CheckOut không được trước CheckIn
        if (checkIn.HasValue && checkOut.HasValue && checkOut <= checkIn)
            throw new ArgumentException("Giờ ra phải sau giờ vào");
    }

    
}