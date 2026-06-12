using NovaStaff.Models.Common;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class PayrollPeriod : BaseEntity
{
    [Key]
    public int PeriodID { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

    public ICollection<PayrollDetail> PayrollDetails { get; set; } = [];
}