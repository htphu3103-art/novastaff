// Models/DTOs/Payroll/PayrollDtos.cs
using NovaStaff.Models.Enums;
using NovaStaff.Models.ValueObjects;

namespace NovaStaff.Models.DTOs.Payroll;

// ============================================================
// PERIOD
// ============================================================

public class PayrollPeriodSummaryDto
{
    public int PeriodID { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PayrollStatus Status { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalNetSalary { get; set; }
}

public class PayrollPeriodDetailDto
{
    public int PeriodID { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PayrollStatus Status { get; set; }
    public List<PayrollDetailDto> Details { get; set; } = [];
}

public class CreatePayrollPeriodRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AdvancePeriodStatusRequest
{
    public int PeriodID { get; set; }
    public PayrollStatus TargetStatus { get; set; }
}

// ============================================================
// DETAIL (PAYSLIP)
// ============================================================

public class PayrollDetailDto
{
    public long DetailID { get; set; }
    public int PeriodID { get; set; }
    public int? EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public decimal BaseSalarySnapshot { get; set; }
    public decimal ActualWorkDays { get; set; }
    public List<BonusAllowanceItem> BonusAndAllowances { get; set; } = [];
    public List<DeductionItem> Deductions { get; set; } = [];
    public decimal TotalIncome { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
    public DateTime? PaidDate { get; set; }
}

public class CalculatePayrollDetailRequest
{
    public int PeriodID { get; set; }
    public int EmployeeID { get; set; }
}

public class BatchCalculateRequest
{
    public int PeriodID { get; set; }
    public int? DepartmentID { get; set; }
}

public class BatchCalculateResult
{
    public int PeriodID { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
// Models/DTOs/Payroll/PayrollItem.cs


// Models/DTOs/Payroll/UpdatePayrollAdjustmentsRequest.cs
public class UpdatePayrollAdjustmentsRequest
{
    public List<BonusAllowanceItem> BonusAndAllowances { get; set; } = [];
    public List<DeductionItem> Deductions { get; set; } = [];
}