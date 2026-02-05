namespace ModulerERP.Inventory.Application.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public Guid? BranchId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public class CreateWarehouseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public Guid? BranchId { get; set; }
    public string? Address { get; set; }
}

public class UpdateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public Guid? BranchId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}
