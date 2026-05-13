// Models/Filters/EmployeeFilter.cs
using NovaStaff.Models.Enums;

namespace NovaStaff.Models.Filters;

public sealed class EmployeeFilter
{
    /// <summary>Tìm theo tên — LIKE %name%</summary>
    public string? NameContains { get; init; }

    /// <summary>Tìm theo mã nhân viên — LIKE %code%</summary>
    public string? CodeContains { get; init; }

    /// <summary>Lọc theo phòng ban</summary>
    public int? DepartmentId { get; init; }

    /// <summary>Lọc theo quản lý trực tiếp</summary>
    public int? SupervisorId { get; init; }

    /// <summary>Lọc theo trạng thái nhân viên</summary>
    public EmployeeStatus? Status { get; init; }

    /// <summary>Lọc theo giới tính</summary>
    public GenderType? Gender { get; init; }

    /// <summary>Lọc theo loại hợp đồng</summary>
    public string? ContractType { get; init; }

    public EmployeeSortField SortBy { get; init; } = EmployeeSortField.FullName;
    public bool SortDescending { get; init; } = false;
}

public enum EmployeeSortField
{
    FullName,
    EmployeeCode,
    JoinDate,
    BaseSalary,
    Department
}