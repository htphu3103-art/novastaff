public record EmployeeDto
{
    public int Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;       // GenderType.ToString()
    public DateTime? BirthDate { get; init; }                 // ← BirthDate (không phải DateOfBirth)
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }                       // ← Phone (không phải PhoneNumber)
    public string? Address { get; init; }
    public string? Position { get; init; }
    public int? JobLevel { get; init; }
    public decimal BaseSalary { get; init; }
    public DateTime? JoinDate { get; init; }                  // ← JoinDate (không phải HireDate)
    public string? ContractType { get; init; }
    public string Status { get; init; } = string.Empty;       // ← Status (không phải IsActive)
    public int? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public int? SupervisorId { get; init; }
    public string? SupervisorName { get; init; }
}