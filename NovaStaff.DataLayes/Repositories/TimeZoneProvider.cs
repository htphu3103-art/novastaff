// NovaStaff.DataLayers/Repositories/TimeZoneProvider.cs

using Microsoft.Extensions.Options;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.Models.Common;

namespace NovaStaff.DataLayers.Repositories;

public sealed class TimeZoneProvider : ITimeZoneProvider
{
    private readonly IDateTimeService _clock;
    private readonly TimeZoneInfo _timeZone;

    public TimeZoneProvider(
        IDateTimeService clock,
        IOptions<AppOptions> options)
    {
        _clock = clock;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            options.Value.DefaultTimeZone);
    }

    // ✅ Trả về DateTimeOffset thay vì DateTime
    // Offset cho biết múi giờ local là +07:00, +09:00... rõ ràng hơn
    public DateTimeOffset LocalNow =>
        TimeZoneInfo.ConvertTime(_clock.UtcNow, _timeZone);

    // ✅ Tiện cho các chỗ chỉ cần "hôm nay theo giờ địa phương"
    public DateOnly TodayLocal =>
    DateOnly.FromDateTime(LocalNow.DateTime);
    // ✅ Convert bất kỳ UTC value nào sang local — dùng khi map DTO
    public DateTimeOffset ToLocal(DateTimeOffset utc) =>
        TimeZoneInfo.ConvertTime(utc, _timeZone);
}