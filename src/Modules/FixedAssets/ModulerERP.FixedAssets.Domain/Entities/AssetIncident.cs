using ModulerERP.SharedKernel.Entities;
using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Domain.Entities;

public class AssetIncident : BaseEntity
{
    public Guid AssetId { get; private set; }
    public Guid? AssignmentId { get; private set; }
    
    public DateTime IncidentDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    public IncidentStatus Status { get; private set; }
    
    public bool IsUserFault { get; private set; }
    public bool DeductFromSalary { get; private set; }
    public decimal? DeductionAmount { get; private set; }

    // Navigation
    public Asset Asset { get; private set; } = null!;
    public AssetAssignment? Assignment { get; private set; }

    private AssetIncident() { }

    public static AssetIncident Create(
        Guid tenantId,
        Guid assetId,
        DateTime incidentDate,
        string description,
        Guid createdByUserId,
        Guid? assignmentId = null)
    {
        var incident = new AssetIncident
        {
            AssetId = assetId,
            IncidentDate = incidentDate,
            Description = description,
            Status = IncidentStatus.Open,
            AssignmentId = assignmentId
        };
        
        incident.SetTenant(tenantId);
        incident.SetCreator(createdByUserId);
        
        return incident;
    }

    public void Resolve(bool isUserFault, bool deductFromSalary, decimal? deductionAmount, Guid updatedByUserId)
    {
        IsUserFault = isUserFault;
        DeductFromSalary = deductFromSalary;
        DeductionAmount = deductionAmount;
        Status = IncidentStatus.Resolved;
        SetUpdater(updatedByUserId);
    }
    
    public void Close(Guid updatedByUserId)
    {
        Status = IncidentStatus.Closed;
        SetUpdater(updatedByUserId);
    }
}
