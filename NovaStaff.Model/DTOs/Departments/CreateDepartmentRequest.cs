// Models/DTOs/Department/CreateDepartmentRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NovaStaff.Models.DTOs.Department;

public record CreateDepartmentRequest
{
    [Required(ErrorMessage = "Tên ph?ng ban không ðý?c ð? tr?ng.")]
    [MaxLength(100, ErrorMessage = "Tên ph?ng ban t?i ða 100 k? t?.")]
    public string Name { get; init; } = string.Empty;

    [MaxLength(20, ErrorMessage = "M? ph?ng ban t?i ða 20 k? t?.")]
    public string? Code { get; init; }

    // null = root department
    public int? ParentId { get; init; }

    [MaxLength(500, ErrorMessage = "Mô t? t?i ða 500 k? t?.")]
    public string? Description { get; init; }

    public int? ManagerEmployeeId { get; init; }
}



