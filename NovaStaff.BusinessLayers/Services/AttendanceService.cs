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
    private readonly IAttendanceNotifier _notifier;
    private readonly ITimeZoneProvider _timeZoneProvider;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IUserRepository userRepo,
        IUnitOfWork uow,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUser,
        IAttendanceNotifier notifier,
        ITimeZoneProvider timeZoneProvider)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _userRepo = userRepo;
        _uow = uow;
        _dateTimeService = dateTimeService;
        _currentUser = currentUser;
        _notifier = notifier;
        _timeZoneProvider = timeZoneProvider;
    }

    // =========================================================
    // MAPPER
    // =========================================================

    private static AttendanceDto MapToDto(AttendanceRecord r) => new()
    {
        RecordId = r.RecordID,
        EmployeeId = r.EmployeeID,
        EmployeeCode = r.Employee?.EmployeeCode,
        EmployeeName = r.Employee?.FullName,
        WorkDate = r.WorkDate,      // DateOnly
        CheckIn = r.CheckIn,       // DateTimeOffset?
        CheckOut = r.CheckOut,      // DateTimeOffset?
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

    public async Task<AttendanceDto?> GetTodayForCurrentUserAsync(
        CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

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

        var records = await _attendanceRepo.GetByEmployeeAndMonthAsync(
            employeeId, year, month, ct);

        return records.Select(MapToDto);
    }

    public async Task<PagedResult<AttendanceDto>> GetPagedAsync(
        AttendanceFilter filter,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

        // ✅ filter.From / filter.To là DateOnly — so sánh với WorkDate (DateOnly) trực tiếp
        Expression<Func<AttendanceRecord, bool>> predicate = r =>
            (!filter.EmployeeId.HasValue || r.EmployeeID == filter.EmployeeId) &&
            (!filter.DepartmentId.HasValue || r.Employee!.DepartmentID == filter.DepartmentId) &&
            (!filter.From.HasValue || r.WorkDate >= filter.From) &&
            (!filter.To.HasValue || r.WorkDate <= filter.To) &&
            (!filter.Status.HasValue || r.Status == filter.Status);

        Func<IQueryable<AttendanceRecord>, IOrderedQueryable<AttendanceRecord>> orderBy =
            filter.SortDescending
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
    // ACTIONS — HR dùng (có employeeId)
    // =========================================================

    public async Task<AttendanceDto> CheckInAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, ct);

        var existing = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (existing != null)
            throw new InvalidOperationException("Nhân viên đã check-in hôm nay");

        // ✅ Lưu UTC, WorkDate dùng TodayUtc (DateOnly)
        var record = new AttendanceRecord
        {
            EmployeeID = employeeId,
            WorkDate = _dateTimeService.TodayUtc,
            CheckIn = _dateTimeService.UtcNow,
            Status = AttendanceStatus.Present,
        };

        await _attendanceRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        var dto = MapToDto(record);
        await _notifier.NotifyCheckInAsync(dto, ct);
        return dto;
    }

    public async Task<AttendanceDto> CheckOutAsync(
        int employeeId,
        CancellationToken ct = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, ct);

        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (record == null)
            throw new InvalidOperationException("Nhân viên chưa check-in hôm nay");

        if (record.CheckOut.HasValue)
            throw new InvalidOperationException("Nhân viên đã check-out hôm nay");

        var tracked = await _attendanceRepo.GetByIdAsync(
            record.RecordID, trackChanges: true, ct: ct);

        // ✅ Lưu UTC
        tracked!.CheckOut = _dateTimeService.UtcNow;

        _attendanceRepo.Update(tracked);
        await _uow.SaveChangesAsync(ct);

        await _attendanceRepo.ReloadAsync(tracked, ct);

        var dto = MapToDto(tracked);
        await _notifier.NotifyCheckOutAsync(dto, ct);
        return dto;
    }

    // =========================================================
    // ACTIONS — Employee tự check-in/out
    // =========================================================

    public async Task<AttendanceDto> CheckInAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var existing = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (existing != null)
            throw new InvalidOperationException("Đã check-in hôm nay");

        // ✅ Lưu UTC, WorkDate dùng TodayUtc (DateOnly)
        var record = new AttendanceRecord
        {
            EmployeeID = employeeId,
            WorkDate = _dateTimeService.TodayUtc,
            CheckIn = _dateTimeService.UtcNow,
            Status = AttendanceStatus.Present,
        };

        await _attendanceRepo.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        var dto = MapToDto(record);
        await _notifier.NotifyCheckInAsync(dto, ct);
        return dto;
    }

    public async Task<AttendanceDto> CheckOutAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var record = await _attendanceRepo.GetTodayAsync(employeeId, ct);
        if (record == null)
            throw new InvalidOperationException("Chưa check-in");

        if (record.CheckOut.HasValue)
            throw new InvalidOperationException("Đã check-out");

        var tracked = await _attendanceRepo.GetByIdAsync(
            record.RecordID, trackChanges: true, ct: ct);

        // ✅ Lưu UTC
        tracked!.CheckOut = _dateTimeService.UtcNow;

        _attendanceRepo.Update(tracked);
        await _uow.SaveChangesAsync(ct);

        await _attendanceRepo.ReloadAsync(tracked, ct);

        var dto = MapToDto(tracked);
        await _notifier.NotifyCheckOutAsync(dto, ct);
        return dto;
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

        // ✅ request.WorkDate là DateOnly — không cần .Date nữa
        var duplicateExists = await _attendanceRepo.ExistsAsync(
            r => r.EmployeeID == request.EmployeeId
              && r.WorkDate == request.WorkDate, ct);

        if (duplicateExists)
            throw new InvalidOperationException(
                $"Đã tồn tại record chấm công ngày " +
                $"{request.WorkDate:dd/MM/yyyy} cho nhân viên này");

        var record = new AttendanceRecord
        {
            EmployeeID = request.EmployeeId,
            WorkDate = request.WorkDate,   // DateOnly
            CheckIn = request.CheckIn,    // DateTimeOffset?
            CheckOut = request.CheckOut,   // DateTimeOffset?
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
        var record = await _attendanceRepo.GetByIdAsync(
            recordId, trackChanges: true, ct: ct);

        if (record == null)
            throw new KeyNotFoundException(
                $"Không tìm thấy record chấm công ID {recordId}");

        ValidateCheckInOut(request.CheckIn, request.CheckOut);

        // ✅ DateTimeOffset?
        record.CheckIn = request.CheckIn;
        record.CheckOut = request.CheckOut;
        record.Status = request.Status;

        _attendanceRepo.Update(record);
        await _uow.SaveChangesAsync(ct);

        await _attendanceRepo.ReloadAsync(record, ct);

        var dto = MapToDto(record);
        await _notifier.NotifyRecordUpdatedAsync(dto, ct);
        return dto;
    }

    public async Task DeleteAsync(long recordId, CancellationToken ct = default)
    {
        var record = await _attendanceRepo.GetByIdAsync(
            recordId, trackChanges: true, ct: ct);

        if (record == null)
            throw new KeyNotFoundException(
                $"Không tìm thấy record chấm công ID {recordId}");

        _attendanceRepo.Delete(record);
        await _uow.SaveChangesAsync(ct);
        await _notifier.NotifyRecordDeletedAsync(recordId, ct);
    }

    // =========================================================
    // PRIVATE HELPERS
    // =========================================================

    private async Task EnsureEmployeeExistsAsync(int employeeId, CancellationToken ct)
    {
        if (!await _employeeRepo.ExistsAsync(employeeId, ct))
            throw new KeyNotFoundException($"Không tìm thấy nhân viên ID {employeeId}");
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

    private static void ValidateYearMonth(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Năm không hợp lệ");

        if (month < 1 || month > 12)
            throw new ArgumentException("Tháng không hợp lệ (1–12)");
    }

    // ✅ DateTime? → DateTimeOffset?
    private static void ValidateCheckInOut(
        DateTimeOffset? checkIn,
        DateTimeOffset? checkOut)
    {
        if (checkIn.HasValue && checkOut.HasValue && checkOut <= checkIn)
            throw new ArgumentException("Giờ ra phải sau giờ vào");
    }
}