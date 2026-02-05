using ModulerERP.SharedKernel.Entities;
using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Domain.Entities;

/// <summary>
/// Asset category for grouping and depreciation rules.
/// </summary>
public class AssetCategory : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    /// <summary>Default depreciation method</summary>
    public DepreciationMethod DepreciationMethod { get; private set; } = DepreciationMethod.StraightLine;
    
    /// <summary>Default useful life in months</summary>
    public int UsefulLifeMonths { get; private set; }
    
    /// <summary>GL Account for asset value</summary>
    public Guid? AssetAccountId { get; private set; }
    
    /// <summary>GL Account for depreciation expense</summary>
    public Guid? DepreciationAccountId { get; private set; }
    
    /// <summary>GL Account for accumulated depreciation</summary>
    public Guid? AccumulatedDepreciationAccountId { get; private set; }

    // Navigation
    public ICollection<Asset> Assets { get; private set; } = new List<Asset>();

    private AssetCategory() { } // EF Core

    public static AssetCategory Create(
        Guid tenantId,
        string code,
        string name,
        int usefulLifeMonths,
        Guid createdByUserId,
        DepreciationMethod method = DepreciationMethod.StraightLine,
        string? description = null)
    {
        var category = new AssetCategory
        {
            Code = code,
            Name = name,
            Description = description,
            DepreciationMethod = method,
            UsefulLifeMonths = usefulLifeMonths
        };

        category.SetTenant(tenantId);
        category.SetCreator(createdByUserId);
        return category;
    }

    public void SetGlAccounts(Guid? assetAccountId, Guid? depreciationAccountId, Guid? accumulatedAccountId)
    {
        AssetAccountId = assetAccountId;
        DepreciationAccountId = depreciationAccountId;
        AccumulatedDepreciationAccountId = accumulatedAccountId;
    }
}
