// Services/Interfaces/IPayrollService.cs
using NovaStaff.Models.Common;
using NovaStaff.Models.DTOs.Payroll;
using NovaStaff.Models.Enums;

namespace NovaStaff.Services.Interfaces;

/// <summary>
/// Service quản lý nghiệp vụ Lương.
///
/// STATE MACHINE (PayrollPeriod):
///   Draft(1) → Calculated(2) → Approved(3) → Paid(4)
///   Chỉ chuyển tiếp, không lùi. Paid = IMMUTABLE.
///
/// SNAPSHOT RULE:
///   BaseSalarySnapshot = Employee.BaseSalary tại thời điểm tính.
///   Sau khi lưu KHÔNG cập nhật dù Employee thay đổi lương.
/// </summary>
public interface IPayrollService
{
    // ── PERIOD ──────────────────────────────────────────────

    /// <summary>Kỳ đang mở (chưa Paid). Null nếu tất cả đã đóng.</summary>
    Task<PayrollPeriodSummaryDto?> GetActivePeriodAsync(CancellationToken ct = default);

    /// <summary>Danh sách tất cả kỳ lương, phân trang, mới nhất trước.</summary>
    Task<PagedResult<PayrollPeriodSummaryDto>> GetPeriodsAsync(
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Chi tiết kỳ lương kèm danh sách phiếu lương.
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    Task<PayrollPeriodDetailDto> GetPeriodDetailAsync(
        int periodId,
        int? departmentId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tạo kỳ lương mới.
    /// Validation: không có kỳ đang mở, Month/Year chưa tồn tại, EndDate >= StartDate.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="InvalidOperationException"/>
    Task<PayrollPeriodSummaryDto> CreatePeriodAsync(
        CreatePayrollPeriodRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Chuyển trạng thái kỳ theo state machine.
    /// Khi → Paid: set PaidDate + Status=Paid cho tất cả PayrollDetail.
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="InvalidOperationException"/>
    Task<PayrollPeriodSummaryDto> AdvancePeriodStatusAsync(
        AdvancePeriodStatusRequest request,
        CancellationToken ct = default);

    // ── DETAIL (PAYSLIP) ─────────────────────────────────────

    /// <summary>Phiếu lương cá nhân của nhân viên trong kỳ.</summary>
    /// <exception cref="KeyNotFoundException"/>
    Task<PayrollDetailDto> GetPayslipAsync(
        int periodId,
        int employeeId,
        CancellationToken ct = default);

    /// <summary>
    /// Tính lương cho một nhân viên trong kỳ (Insert hoặc Update).
    /// IMMUTABLE: không cho phép nếu kỳ đã Paid.
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    /// <exception cref="InvalidOperationException"/>
    Task<PayrollDetailDto> CalculatePayslipAsync(
        CalculatePayrollDetailRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Tính lương hàng loạt cho nhân viên CHƯA có phiếu lương trong kỳ.
    /// Lỗi từng cá nhân không rollback cả batch.
    /// </summary>
    Task<BatchCalculateResult> BatchCalculateAsync(
        BatchCalculateRequest request,
        CancellationToken ct = default);

    /// <summary>Tổng NetSalary của kỳ — dùng cho Dashboard / Budget.</summary>
    Task<decimal> GetTotalNetSalaryAsync(int periodId, CancellationToken ct = default);

    /// <summary>Đếm phiếu lương theo trạng thái — dùng cho HR tracking.</summary>
    Task<Dictionary<PayrollStatus, int>> GetStatusSummaryAsync(
        int periodId,
        CancellationToken ct = default);

    Task<PayrollDetailDto> UpdateAdjustmentsAsync(
    int periodId,
    int employeeId,
    UpdatePayrollAdjustmentsRequest request,
    CancellationToken ct = default);
}