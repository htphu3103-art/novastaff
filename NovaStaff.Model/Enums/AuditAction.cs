using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.Enums
{
    public enum AuditAction : byte
    {
        Unknown = 0,
        Insert = 1,
        Update = 2,
        Delete = 3,
        SoftDelete = 4
    }
}





