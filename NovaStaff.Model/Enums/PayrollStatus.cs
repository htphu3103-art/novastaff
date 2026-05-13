using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Enums
{
    public enum PayrollStatus : byte
    {
        Unknown = 0,
        Draft = 1,
        Calculated = 2,
        Approved = 3,
        Paid = 4
    }
}





