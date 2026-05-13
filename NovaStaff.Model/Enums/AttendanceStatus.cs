using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Enums
{
    public enum AttendanceStatus : byte
    {
        Unknown = 0,
        Present = 1,
        Late = 2,
        Absent = 3,
        HalfDay = 4,
        Leave = 5
    }
}
