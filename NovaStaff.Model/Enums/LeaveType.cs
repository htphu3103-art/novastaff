using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Enums
{
    public enum LeaveType : byte
    {
        Unknown = 0,
        Annual = 1,      // Ngh? phép nãm
        Sick = 2,        // Ngh? b?nh
        Maternity = 3,   // Ngh? thai s?n
        Unpaid = 4,      // Ngh? không lýõng
        Compensatory = 5 // Ngh? bù
    }
}





