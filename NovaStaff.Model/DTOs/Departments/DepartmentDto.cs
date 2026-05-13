public record DepartmentDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Code { get; init; }

    public short? Level { get; init; }

    public int? ParentId { get; init; }     

    public bool? HasChildren { get; set; }

    public bool IsActive { get; init; }

    public string? Description { get; init; }

    public int? ManagerId { get; init; }

    public string? ManagerName { get; init; } 
}



