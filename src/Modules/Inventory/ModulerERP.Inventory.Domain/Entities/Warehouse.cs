using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

public enum WarehouseType
{
    Central = 1,
    ProjectSite = 2,
    Transit = 3,
    Quarantine = 4
}

/// <summary>
/// Physical storage locations.
/// </summary>
public class Warehouse : BaseEntity
{
    /// <summary>Short code (e.g., 'WH-KYRENIA')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    /// <summary>Is this the primary warehouse?</summary>
    public bool IsDefault { get; private set; }
    
    public WarehouseType Type { get; private set; } = WarehouseType.Central;

    /// <summary>For HR/Branch integration</summary>
    public Guid? BranchId { get; private set; }
    
    /// <summary>Linked Project for ProjectSite warehouses</summary>
    public Guid? ProjectId { get; private set; }
    
    /// <summary>Warehouse address as JSON</summary>
    public string? Address { get; private set; }



    // Navigation
    public ICollection<WarehouseLocation> Locations { get; private set; } = new List<WarehouseLocation>();
    public ICollection<StockLevel> StockLevels { get; private set; } = new List<StockLevel>();

    private Warehouse() { } // EF Core

    public static Warehouse Create(
        Guid tenantId,
        string code,
        string name,
        Guid createdByUserId,
        WarehouseType type = WarehouseType.Central,
        string? description = null,
        bool isDefault = false,
        Guid? branchId = null,
        Guid? projectId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (type == WarehouseType.ProjectSite && projectId == null)
             throw new ArgumentException("ProjectId is required for ProjectSite warehouses", nameof(projectId));

        var warehouse = new Warehouse
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Type = type,
            Description = description,
            IsDefault = isDefault,
            BranchId = branchId,
            ProjectId = projectId
        };

        warehouse.SetTenant(tenantId);
        warehouse.SetCreator(createdByUserId);
        return warehouse;
    }

    public void Update(string name, string? description, WarehouseType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }

    public void SetAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;
    public void SetAddress(string address) => Address = address;
    

}
