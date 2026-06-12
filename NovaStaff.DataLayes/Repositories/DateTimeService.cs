using NovaStaff.DataLayers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.DataLayers.Repositories
{
    public sealed class DateTimeService : IDateTimeService
    {
        // ✅ DateTime → DateTimeOffset
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        // ✅ Tiện dùng khi chỉ cần ngày, không cần giờ
        public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
