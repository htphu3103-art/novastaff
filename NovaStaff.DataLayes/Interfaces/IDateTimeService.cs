using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Interfaces/IDateTimeService.cs
namespace NovaStaff.DataLayers.Interfaces;

/// <summary>
/// Tr?u tý?ng hoá vi?c l?y th?i gian hi?n t?i.
/// 
/// T?i sao không dùng DateTime.UtcNow tr?c ti?p?
///   V?n ð? v?i Unit Test: DateTime.Now là static, không th? mock.
///   N?u code dùng DateTime.UtcNow ? test luôn ph? thu?c vào th?i gian th?c
///   ? không th? test "hành vi khi h?p ð?ng h?t h?n vào ngày X" m?t cách ?n ð?nh.
/// 
///   V?i IDateTimeService: inject FakeDateTimeService trong test,
///   tr? v? b?t k? th?i ði?m nào mu?n ? test hoàn toàn deterministic.
/// 
/// Implementation th?c t? (production):
///   public class SystemDateTimeService : IDateTimeService {
///       public DateTime UtcNow => DateTime.UtcNow;
///       public DateTime LocalNow => DateTime.Now;
///   }
/// 
/// Implementation cho test:
///   public class FakeDateTimeService : IDateTimeService {
///       public DateTime UtcNow { get; set; } = new DateTime(2025, 1, 1);
///       public DateTime LocalNow => UtcNow.ToLocalTime();
///   }
/// </summary>
public interface IDateTimeService
{
    DateTimeOffset UtcNow { get; }
    DateOnly TodayUtc { get; }
}




