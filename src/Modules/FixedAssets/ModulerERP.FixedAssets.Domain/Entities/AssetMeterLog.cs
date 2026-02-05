using ModulerERP.SharedKernel.Entities;
using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Domain.Entities;

public class AssetMeterLog : BaseEntity
{
    public Guid AssetId { get; private set; }
    public Guid? AssignmentId { get; private set; }
    
    public DateTime LogDate { get; private set; }
    public decimal MeterValue { get; private set; }
    
    public MeterLogSource Source { get; private set; }

    // Navigation
    public Asset Asset { get; private set; } = null!;
    public AssetAssignment? Assignment { get; private set; }

    private AssetMeterLog() { }

    public static AssetMeterLog Create(
        Guid tenantId,
        Guid assetId,
        DateTime logDate,
        decimal meterValue,
        MeterLogSource source,
        Guid createdByUserId,
        Guid? assignmentId = null)
    {
        var log = new AssetMeterLog
        {
            AssetId = assetId,
            LogDate = logDate,
            MeterValue = meterValue,
            Source = source,
            AssignmentId = assignmentId
        };
        
        log.SetTenant(tenantId);
        log.SetCreator(createdByUserId);
        
        return log;
    }
}
