using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Inventory.Domain.Entities;

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
    
    /// <summary>For HR/Branch integration</summary>
    public Guid? BranchId { get; private set; }
    
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
        string? description = null,
        bool isDefault = false,
        Guid? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var warehouse = new Warehouse
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            IsDefault = isDefault,
            BranchId = branchId
        };

        warehouse.SetTenant(tenantId);
        warehouse.SetCreator(createdByUserId);
        return warehouse;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void SetAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;
    public void SetAddress(string address) => Address = address;
}
