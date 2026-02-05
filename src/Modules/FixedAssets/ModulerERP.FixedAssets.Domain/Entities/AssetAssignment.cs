using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.FixedAssets.Domain.Entities;

public class AssetAssignment : BaseEntity
{
    public Guid AssetId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Guid? LocationId { get; private set; }
    
    public DateTime AssignedDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    
    public decimal StartValue { get; private set; }
    public decimal? EndValue { get; private set; }
    
    public string? Condition { get; private set; }

    // Navigation
    public Asset Asset { get; private set; } = null!;

    private AssetAssignment() { }

    public static AssetAssignment Create(
        Guid tenantId,
        Guid assetId,
        Guid? employeeId,
        Guid? locationId,
        decimal startValue,
        Guid createdByUserId,
        string? condition = null)
    {
        var assignment = new AssetAssignment
        {
            AssetId = assetId,
            EmployeeId = employeeId,
            LocationId = locationId,
            AssignedDate = DateTime.UtcNow,
            StartValue = startValue,
            Condition = condition
        };
        
        assignment.SetTenant(tenantId);
        assignment.SetCreator(createdByUserId);
        
        return assignment;
    }

    public void Return(DateTime returnedDate, decimal endValue, string condition, Guid updatedByUserId)
    {
        ReturnedDate = returnedDate;
        EndValue = endValue;
        Condition = condition;
        SetUpdater(updatedByUserId);
    }
}
