using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.FixedAssets.Domain.Entities;

public class AssetMaintenance : BaseEntity
{
    public Guid AssetId { get; private set; }
    public Guid? IncidentId { get; private set; }
    public Guid SupplierId { get; private set; }
    
    public DateTime ServiceDate { get; private set; }
    public decimal Cost { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    public DateTime? NextServiceDate { get; private set; }
    public decimal? NextServiceMeter { get; private set; }

    // Navigation
    public Asset Asset { get; private set; } = null!;
    public AssetIncident? Incident { get; private set; }

    private AssetMaintenance() { }

    public static AssetMaintenance Create(
        Guid tenantId,
        Guid assetId,
        Guid supplierId,
        DateTime serviceDate,
        decimal cost,
        string description,
        Guid createdByUserId,
        Guid? incidentId = null,
        DateTime? nextServiceDate = null,
        decimal? nextServiceMeter = null)
    {
        var maintenance = new AssetMaintenance
        {
            AssetId = assetId,
            SupplierId = supplierId,
            ServiceDate = serviceDate,
            Cost = cost,
            Description = description,
            IncidentId = incidentId,
            NextServiceDate = nextServiceDate,
            NextServiceMeter = nextServiceMeter
        };
        
        maintenance.SetTenant(tenantId);
        maintenance.SetCreator(createdByUserId);
        
        return maintenance;
    }
}
