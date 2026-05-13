using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.ValueObjects;

/// <summary>
/// M?t kho?n kh?u tr? trong b?ng lýõng.
/// Ðý?c serialize thành JSON lýu vào c?t DeductionsJson.
/// </summary>
public class DeductionItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}




