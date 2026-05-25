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
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime LocalNow => DateTime.Now;
    }
}
