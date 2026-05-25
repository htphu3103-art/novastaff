using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.ValueObjects;

/// <summary>
/// M?t kho?n ph? c?p ho?c thý?ng trong b?ng lýõng.
/// Ðý?c serialize thành JSON lýu vào c?t BonusAndAllowancesJson.
/// </summary>
public class BonusAllowanceItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}




