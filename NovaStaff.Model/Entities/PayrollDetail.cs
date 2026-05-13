using NovaStaff.Models.Common;
using NovaStaff.Models.Enums;
using NovaStaff.Models.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using NovaStaff.Shared.Serialization;

namespace NovaStaff.Models.Entities
{
    [Table("PayrollDetails")]
    public class PayrollDetail : BaseEntity
    {
        [Key]
        public long DetailID { get; set; }

        public int PeriodID { get; set; }
        public int? EmployeeID { get; set; }

        public decimal BaseSalarySnapshot { get; set; }
        public decimal ActualWorkDays { get; set; }

        public string? BonusAndAllowancesJson { get; set; }
        public string? DeductionsJson { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal NetSalary { get; set; }

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
        public DateTime? PaidDate { get; set; }

        // ?? JSON helpers — NotMapped, không lýu vào DB ?????????????????????????

        [NotMapped]
        public List<BonusAllowanceItem> BonusAndAllowances
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BonusAndAllowancesJson))
                    return [];

                try
                {
                    return JsonSerializer.Deserialize<List<BonusAllowanceItem>>(
                                                        BonusAndAllowancesJson,
                                                        SystemJson.Default
                                                    ) ?? [];
                }
                catch (JsonException)
                {
                    // JSON b? corrupt trong DB — tr? v? r?ng thay v? crash app
                    return [];
                }
            }
            set => BonusAndAllowancesJson =
    JsonSerializer.Serialize(value, SystemJson.Default);
        }

        [NotMapped]
        public List<DeductionItem> Deductions
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DeductionsJson))
                    return [];

                try
                {
                    return JsonSerializer.Deserialize<List<DeductionItem>>(
                            DeductionsJson,
                            SystemJson.Default
                        ) ?? [];
                }
                catch (JsonException)
                {
                    return [];
                }
            }
            set => DeductionsJson =
    JsonSerializer.Serialize(value, SystemJson.Default);
        }

        // ?? Navigation ??????????????????????????????????????????????????????????
        public virtual PayrollPeriod? Period { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}




