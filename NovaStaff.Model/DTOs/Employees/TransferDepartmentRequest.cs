using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace NovaStaff.Models.DTOs.Employees
{
    public record TransferDepartmentRequest
    {
        [Required(ErrorMessage = "Phòng ban không được để trống.")]
        public int NewDepartmentId { get; init; }
    }
}
