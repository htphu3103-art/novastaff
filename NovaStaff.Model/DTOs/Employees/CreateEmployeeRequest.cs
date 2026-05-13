using NovaStaff.Models.Enums;
using System.ComponentModel.DataAnnotations;

public record CreateEmployeeRequest
{
    [Required(ErrorMessage = "Mã nhân viên không được để trống.")]
    [MaxLength(20, ErrorMessage = "Mã nhân viên tối đa 20 ký tự.")]
    public string EmployeeCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; init; } = string.Empty;

    public GenderType Gender { get; init; } = GenderType.Other;

    public DateTime? BirthDate { get; init; }                 // ← BirthDate

    [Required(ErrorMessage = "Email không được để trống.")]
    [MaxLength(150, ErrorMessage = "Email tối đa 150 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    public string? Phone { get; init; }                       // ← Phone

    [MaxLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự.")]
    public string? Address { get; init; }

    public int? DepartmentId { get; init; }
    public int? SupervisorId { get; init; }

    [MaxLength(100, ErrorMessage = "Chức vụ tối đa 100 ký tự.")]
    public string? Position { get; init; }

    public int? JobLevel { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản không được âm.")]
    public decimal BaseSalary { get; init; } = 0;

    public DateTime? JoinDate { get; init; }                  // ← JoinDate

    [MaxLength(50, ErrorMessage = "Loại hợp đồng tối đa 50 ký tự.")]
    public string? ContractType { get; init; }
}