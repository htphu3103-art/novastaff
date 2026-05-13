using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NovaStaff.Models.Entities
{
    /// <summary>
    /// K? tính lýõng (tháng/nãm)
    /// </summary>
    [Table("PayrollPeriods")]
    public class PayrollPeriod : BaseEntity
    {
        [Key]
        public int PeriodID { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;


        // Navigation
        public ICollection<PayrollDetail> PayrollDetails { get; set; } = [];
    }
}




