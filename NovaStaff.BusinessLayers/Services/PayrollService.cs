// BusinessLayers/Services/PayrollService.cs
using Microsoft.EntityFrameworkCore;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Payroll;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using NovaStaff.Models.Exceptions;
using NovaStaff.Models.ValueObjects;
using NovaStaff.Services.Interfaces;
using NovaStaff.Shared.Serialization;
using System.Text.Json;


namespace NovaStaff.BusinessLayers.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepo;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTimeService;

    // Số ngày công chuẩn — move vào IConfiguration nếu cần thay đổi theo kỳ
    private const decimal StandardWorkDays = 26m;

    public PayrollService(
        IPayrollRepository payrollRepo,
        IUnitOfWork uow,
        IDateTimeService dateTimeService)
    {
        _payrollRepo = payrollRepo;
        _uow = uow;
        _dateTimeService = dateTimeService;
    }

    // ============================================================
    // MAPPER — static, không allocate thêm object
    // ============================================================

    private static PayrollPeriodSummaryDto MapToSummaryDto(
        PayrollPeriod p, int totalEmployees, decimal totalNetSalary) => new()
        {
            PeriodID = p.PeriodID,
            Month = p.Month,
            Year = p.Year,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            TotalEmployees = totalEmployees,
            TotalNetSalary = totalNetSalary
        };

    private static PayrollDetailDto MapToDetailDto(PayrollDetail d) => new()
    {
        DetailID = d.DetailID,
        PeriodID = d.PeriodID,
        EmployeeID = d.EmployeeID,
        EmployeeCode = d.Employee?.EmployeeCode ?? string.Empty,
        FullName = d.Employee?.FullName ?? string.Empty,
        DepartmentName = d.Employee?.Department?.DepartmentName,
        BaseSalarySnapshot = d.BaseSalarySnapshot,
        ActualWorkDays = d.ActualWorkDays,
        BonusAndAllowances = d.BonusAndAllowances,
        Deductions = d.Deductions,
        TotalIncome = d.TotalIncome,
        NetSalary = d.NetSalary,
        Status = d.Status,
        PaidDate = d.PaidDate
    };

    // ============================================================
    // PERIOD — READ
    // ============================================================

    public async Task<PayrollPeriodSummaryDto?> GetActivePeriodAsync(CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetActiveAsync(ct);
        if (period is null) return null;

        var total = await _payrollRepo.GetTotalNetSalaryAsync(period.PeriodID, ct);
        var counts = await _payrollRepo.CountByStatusAsync(period.PeriodID, ct);

        return MapToSummaryDto(period, counts.Values.Sum(), total);
    }

    public async Task<PagedResult<PayrollPeriodSummaryDto>> GetPeriodsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        var paged = await _payrollRepo.GetPagedAsync(
            pageIndex,
            pageSize,
            filter: null,
            orderBy: q => q.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month),
            ct: ct);

        // N+1 chấp nhận được — pageSize thường <= 12 (12 tháng/năm)
        // Materialize 1 lần để tránh enumerate IEnumerable nhiều lần
        var periodList = paged.Items.ToList();
        var dtos = new List<PayrollPeriodSummaryDto>(periodList.Count);
        foreach (var period in periodList)
        {
            var total = await _payrollRepo.GetTotalNetSalaryAsync(period.PeriodID, ct);
            var counts = await _payrollRepo.CountByStatusAsync(period.PeriodID, ct);
            dtos.Add(MapToSummaryDto(period, counts.Values.Sum(), total));
        }

        return new PagedResult<PayrollPeriodSummaryDto>(
            dtos, paged.TotalCount, pageIndex, pageSize);
    }

    public async Task<PayrollPeriodDetailDto> GetPeriodDetailAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetByIdAsync(periodId, ct: ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy kỳ lương ID {periodId}");

        var details = await _payrollRepo.GetDetailsByPeriodAsync(periodId, departmentId, ct);

        return new PayrollPeriodDetailDto
        {
            PeriodID = period.PeriodID,
            Month = period.Month,
            Year = period.Year,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            Status = period.Status,
            Details = details.Select(MapToDetailDto).ToList()
        };
    }

    // ============================================================
    // PERIOD — WRITE
    // ============================================================

    public async Task<PayrollPeriodSummaryDto> CreatePeriodAsync(
        CreatePayrollPeriodRequest request,
        CancellationToken ct = default)
    {
        // 1. VALIDATION
        if (request.Month is < 1 or > 12)
            throw new ArgumentException("Tháng phải từ 1 đến 12");

        if (request.Year < 2000)
            throw new ArgumentException("Năm không hợp lệ");

        if (request.EndDate < request.StartDate)
            throw new ArgumentException("EndDate phải lớn hơn hoặc bằng StartDate");

        // 2. BUSINESS RULE: chỉ 1 kỳ đang mở
        var activePeriod = await _payrollRepo.GetActiveAsync(ct);
        if (activePeriod is not null)
            throw new ConflictException(
                $"Đang có kỳ lương chưa đóng (Tháng {activePeriod.Month}/{activePeriod.Year}, " +
                $"Status={activePeriod.Status}). Vui lòng đóng kỳ hiện tại trước khi tạo mới.");

        // 3. UNIQUE CHECK: Month/Year
        var duplicate = await _payrollRepo.ExistsAsync(
            p => p.Month == request.Month && p.Year == request.Year, ct);
        if (duplicate)
            throw new ConflictException(
                $"Kỳ lương Tháng {request.Month}/{request.Year} đã tồn tại");

        // 4. CREATE
        var period = new PayrollPeriod
        {
            Month = request.Month,
            Year = request.Year,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = PayrollStatus.Draft
        };

        try
        {
            await _payrollRepo.AddAsync(period, ct);
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? "";
            if (msg.Contains("IX_PayrollPeriods_Month_Year"))
                throw new ConflictException(
                    $"Kỳ lương Tháng {request.Month}/{request.Year} đã tồn tại");
            throw;
        }

        return MapToSummaryDto(period, 0, 0m);
    }

    public async Task<PayrollPeriodSummaryDto> AdvancePeriodStatusAsync(
        AdvancePeriodStatusRequest request,
        CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetByIdAsync(request.PeriodID, trackChanges: true, ct: ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy kỳ lương ID {request.PeriodID}");

        // Validate state machine
        ValidateStatusTransition(period.Status, request.TargetStatus);

        period.Status = request.TargetStatus;

        // Khi chuyển → Paid: đánh PaidDate + Status cho toàn bộ PayrollDetail
        if (request.TargetStatus == PayrollStatus.Paid)
            await MarkAllDetailsPaidAsync(request.PeriodID, ct);

        await _uow.SaveChangesAsync(ct);

        var total = await _payrollRepo.GetTotalNetSalaryAsync(period.PeriodID, ct);
        var counts = await _payrollRepo.CountByStatusAsync(period.PeriodID, ct);

        return MapToSummaryDto(period, counts.Values.Sum(), total);
    }

    // ============================================================
    // DETAIL — READ
    // ============================================================

    public async Task<PayrollDetailDto> GetPayslipAsync(
        int periodId,
        int employeeId,
        CancellationToken ct = default)
    {
        var detail = await _payrollRepo.GetDetailByEmployeeAsync(
    periodId,
    employeeId,
    trackChanges: true,
    ct)
    ?? throw new KeyNotFoundException(
        $"Không tìm thấy phiếu lương (PeriodID={periodId}, EmployeeID={employeeId})");

        return MapToDetailDto(detail);
    }

    // ============================================================
    // DETAIL — WRITE
    // ============================================================

    public async Task<PayrollDetailDto> CalculatePayslipAsync(
        CalculatePayrollDetailRequest request,
        CancellationToken ct = default)
    {
        // 1. Kiểm tra kỳ tồn tại + chưa Paid
        var period = await _payrollRepo.GetByIdAsync(request.PeriodID, ct: ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy kỳ lương ID {request.PeriodID}");

        if (period.Status == PayrollStatus.Paid)
            throw new ConflictException(
                $"Kỳ lương ID {request.PeriodID} đã Paid và không thể chỉnh sửa");

        // 2. Lấy Employee kèm Department (cần BaseSalary để snapshot)
        var employee = await _uow.Repository<Employee, int>()
            .GetByIdAsync(
                request.EmployeeID,
                include: q => q.Include(e => e.Department),
                ct: ct)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy nhân viên ID {request.EmployeeID}");

        // 3. Số ngày công thực tế
        decimal actualWorkDays = await GetActualWorkDaysAsync(
            request.EmployeeID, period.StartDate, period.EndDate, ct);

        // 4. Tính lương — SNAPSHOT BaseSalary tại thời điểm này
        decimal baseSalarySnapshot = employee.BaseSalary;
        decimal proportionalBase = baseSalarySnapshot / StandardWorkDays * actualWorkDays;
        decimal netSalary = Math.Max(0m, proportionalBase); // CK_PayrollDetail_NetSalary

        // 5. Upsert
        var existing = await _payrollRepo.GetDetailByEmployeeAsync(
    request.PeriodID,
    request.EmployeeID,
    trackChanges: true,
    ct);

        var detailRepo =
            _uow.Repository<PayrollDetail, long>();

        PayrollDetail detail;

        if (existing is not null)
        {
            detail = existing;

            decimal bonus =
                detail.BonusAndAllowances.Sum(x => x.Amount);

            decimal deduction =
                detail.Deductions.Sum(x => x.Amount);

            detail.BaseSalarySnapshot = baseSalarySnapshot;
            detail.ActualWorkDays = actualWorkDays;

            detail.TotalIncome =
                proportionalBase + bonus;

            detail.NetSalary = Math.Max(
                0m,
                detail.TotalIncome - deduction);

            detail.Status = PayrollStatus.Calculated;
        }
        else
        {
            detail = new PayrollDetail
            {
                PeriodID = request.PeriodID,
                EmployeeID = request.EmployeeID,
                BaseSalarySnapshot = baseSalarySnapshot,
                ActualWorkDays = actualWorkDays,

                BonusAndAllowancesJson = "[]",
                DeductionsJson = "[]",

                TotalIncome = proportionalBase,
                NetSalary = netSalary,
                Status = PayrollStatus.Calculated
            };

            await detailRepo.AddAsync(detail, ct);
        }

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? "";
            if (msg.Contains("IX_PayrollDetails_Employee_Period_Unique"))
                throw new ConflictException(
                    $"Phiếu lương của nhân viên ID {request.EmployeeID} trong kỳ này đã tồn tại");
            throw;
        }

        // Gắn navigation để map DTO (entity vừa Insert chưa có navigation)
        detail.Employee = employee;
        detail.Period = period;

        return MapToDetailDto(detail);
    }

    public async Task<BatchCalculateResult> BatchCalculateAsync(
        BatchCalculateRequest request,
        CancellationToken ct = default)
    {
        // Kiểm tra kỳ trước khi chạy batch
        var period = await _payrollRepo.GetByIdAsync(request.PeriodID, ct: ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy kỳ lương ID {request.PeriodID}");

        if (period.Status == PayrollStatus.Paid)
            throw new ConflictException(
                $"Kỳ lương ID {request.PeriodID} đã Paid và không thể chỉnh sửa");

        var missing = await _payrollRepo.GetMissingDetailsAsync(
            request.PeriodID, request.DepartmentID, ct);

        int successCount = 0, skippedCount = 0;
        var errors = new List<string>();

        foreach (var employee in missing)
        {
            try
            {
                await CalculatePayslipAsync(new CalculatePayrollDetailRequest
                {
                    PeriodID = request.PeriodID,
                    EmployeeID = employee.EmployeeID
                }, ct);

                successCount++;
            }
            catch (AppException ex)
            {
                skippedCount++;
                errors.Add($"[{employee.EmployeeCode}] {employee.FullName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"[{employee.EmployeeCode}] {employee.FullName}: {ex.Message}");
            }
        }

        return new BatchCalculateResult
        {
            PeriodID = request.PeriodID,
            SuccessCount = successCount,
            SkippedCount = skippedCount,
            Errors = errors
        };
    }

    public async Task<PayrollDetailDto> UpdateAdjustmentsAsync(
    int periodId,
    int employeeId,
    UpdatePayrollAdjustmentsRequest request,
    CancellationToken ct = default)
    {
        Console.WriteLine(JsonSerializer.Serialize(request.BonusAndAllowances));
        // 1. Kiểm tra kỳ lương
        var period = await _payrollRepo.GetByIdAsync(periodId, ct: ct)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy kỳ lương ID {periodId}");

        if (period.Status == PayrollStatus.Paid)
            throw new ConflictException(
                "Kỳ lương đã Paid, không thể chỉnh sửa");

        // 2. Lấy tracked entity
        var detail = await _payrollRepo.GetDetailByEmployeeAsync(
            periodId,
            employeeId,
            trackChanges: true,
            ct)
            ?? throw new KeyNotFoundException(
                $"Chưa có phiếu lương cho nhân viên ID {employeeId}. " +
                "Vui lòng tính lương trước khi điều chỉnh.");

        // 3. Validate
        if (request.BonusAndAllowances.Any(x => x.Amount < 0))
            throw new ArgumentException(
                "Phụ cấp/thưởng không được âm");

        if (request.Deductions.Any(x => x.Amount < 0))
            throw new ArgumentException(
                "Khấu trừ không được âm");

        // 4. Update JSON fields (FIXED)
        detail.BonusAndAllowancesJson =
            JsonSerializer.Serialize(request.BonusAndAllowances, SystemJson.Default);

        detail.DeductionsJson =
            JsonSerializer.Serialize(request.Deductions, SystemJson.Default);
        // 5. Recalculate
        decimal bonus = detail.BonusAndAllowances.Sum(x => x.Amount);

        decimal deduction = detail.Deductions.Sum(x => x.Amount);

        decimal proportionalBase =
            detail.BaseSalarySnapshot
            / StandardWorkDays
            * detail.ActualWorkDays;

        detail.TotalIncome = proportionalBase + bonus;

        detail.NetSalary = Math.Max(
            0m,
            detail.TotalIncome - deduction);

        // 6. Persist
        await _uow.SaveChangesAsync(ct);
        Console.WriteLine(detail.BonusAndAllowancesJson);
        Console.WriteLine(JsonSerializer.Serialize(detail.BonusAndAllowances, SystemJson.Default));
        var debugJson = JsonSerializer.Serialize(detail.BonusAndAllowances, SystemJson.Default);
        Console.WriteLine(debugJson);
        // 7. Return DTO
        return MapToDetailDto(detail);
    }
    public Task<decimal> GetTotalNetSalaryAsync(int periodId, CancellationToken ct = default)
        => _payrollRepo.GetTotalNetSalaryAsync(periodId, ct);

    public Task<Dictionary<PayrollStatus, int>> GetStatusSummaryAsync(
        int periodId,
        CancellationToken ct = default)
        => _payrollRepo.CountByStatusAsync(periodId, ct);

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    /// <summary>
    /// Validate state machine: Draft→Calculated→Approved→Paid
    /// Chỉ chuyển tiếp theo đúng thứ tự, không lùi, không nhảy cóc.
    /// </summary>
    private static void ValidateStatusTransition(PayrollStatus current, PayrollStatus target)
    {
        var validNext = new Dictionary<PayrollStatus, PayrollStatus>
        {
            [PayrollStatus.Draft] = PayrollStatus.Calculated,
            [PayrollStatus.Calculated] = PayrollStatus.Approved,
            [PayrollStatus.Approved] = PayrollStatus.Paid,
        };

        if (!validNext.TryGetValue(current, out var expected) || expected != target)
        {
            var nextLabel = validNext.TryGetValue(current, out var next)
                ? next.ToString()
                : "không có (đã Paid)";

            throw new ConflictException(
                $"Không thể chuyển trạng thái từ {current} sang {target}. " +
                $"Trạng thái tiếp theo hợp lệ: {nextLabel}");
        }
    }

    /// <summary>
    /// Đánh PaidDate + Status=Paid cho toàn bộ PayrollDetail của kỳ.
    /// Gọi khi AdvancePeriodStatus → Paid.
    /// </summary>
    private async Task MarkAllDetailsPaidAsync(int periodId, CancellationToken ct)
    {
        var details = await _payrollRepo.GetDetailsByPeriodAsync(periodId, null, ct);
        var detailRepo = _uow.Repository<PayrollDetail, long>();
        var paidAt = _dateTimeService.UtcNow;

        foreach (var d in details)
        {
            var tracked = await detailRepo.GetByIdAsync(d.DetailID, trackChanges: true, ct: ct);
            if (tracked is null) continue;

            tracked.Status = PayrollStatus.Paid;
            tracked.PaidDate = paidAt;
            detailRepo.Update(tracked);
        }
    }

    /// <summary>
    /// Số ngày công thực tế của nhân viên trong kỳ.
    /// TODO: Query AttendanceRecord khi IAttendanceRepository được inject.
    /// Hiện tại fallback về StandardWorkDays.
    /// </summary>
    private async Task<decimal> GetActualWorkDaysAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        // var count = await _uow.Repository<AttendanceRecord, long>()
        //     .CountAsync(a =>
        //         a.EmployeeID == employeeId &&
        //         a.Date >= DateOnly.FromDateTime(startDate) &&
        //         a.Date <= DateOnly.FromDateTime(endDate) &&
        //         a.Status == AttendanceStatus.Present, ct);
        // return count > 0 ? count : StandardWorkDays;

        await Task.CompletedTask;
        return StandardWorkDays;
    }
}