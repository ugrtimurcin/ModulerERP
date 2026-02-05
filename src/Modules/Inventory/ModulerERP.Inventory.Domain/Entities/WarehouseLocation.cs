namespace ModulerERP.Inventory.Domain.Entities;

/// <summary>
/// Sub-locations within warehouses (Aisles, Racks, Bins).
/// </summary>
public class WarehouseLocation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WarehouseId { get; private set; }
    public Guid TenantId { get; private set; }
    
    /// <summary>Location code (e.g., 'A-01-01')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public string? Name { get; private set; }
    
    /// <summary>Aisle identifier</summary>
    public string? Aisle { get; private set; }
    
    /// <summary>Rack/Shelf identifier</summary>
    public string? Rack { get; private set; }
    
    /// <summary>Bin/Position identifier</summary>
    public string? Bin { get; private set; }

    // Navigation
    public Warehouse? Warehouse { get; private set; }

    private WarehouseLocation() { } // EF Core

    public static WarehouseLocation Create(
        Guid tenantId,
        Guid warehouseId,
        string code,
        string? name = null,
        string? aisle = null,
        string? rack = null,
        string? bin = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        return new WarehouseLocation
        {
            TenantId = tenantId,
            WarehouseId = warehouseId,
            Code = code.ToUpperInvariant(),
            Name = name,
            Aisle = aisle,
            Rack = rack,
            Bin = bin
        };
    }

    public void Update(string code, string? name, string? aisle, string? rack, string? bin)
    {
        Code = code.ToUpperInvariant();
        Name = name;
        Aisle = aisle;
        Rack = rack;
        Bin = bin;
    }
}
