using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Cost centers for departmental accounting.
/// </summary>
public class CostCenter : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public Guid? ParentCostCenterId { get; private set; }
    
    /// <summary>Is this a header (non-postable)?</summary>
    public bool IsHeader { get; private set; }

    // Navigation
    public CostCenter? ParentCostCenter { get; private set; }
    public ICollection<CostCenter> ChildCostCenters { get; private set; } = new List<CostCenter>();

    private CostCenter() { } // EF Core

    public static CostCenter Create(
        Guid tenantId,
        string code,
        string name,
        Guid createdByUserId,
        string? description = null,
        Guid? parentCostCenterId = null,
        bool isHeader = false)
    {
        var costCenter = new CostCenter
        {
            Code = code,
            Name = name,
            Description = description,
            ParentCostCenterId = parentCostCenterId,
            IsHeader = isHeader
        };

        costCenter.SetTenant(tenantId);
        costCenter.SetCreator(createdByUserId);
        return costCenter;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
