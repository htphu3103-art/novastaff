using NovaStaff.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.DTOs.Employees
{
    public class ChangeEmployeeStatusRequest
    {
        public EmployeeStatus Status { get; set; }
    }
}
