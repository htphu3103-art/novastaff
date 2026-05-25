using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Enums
{
    public enum UserRole : byte
    {
        Unknown = 0,
        Admin = 1,
        Manager = 2,
        Staff = 3
    }
}