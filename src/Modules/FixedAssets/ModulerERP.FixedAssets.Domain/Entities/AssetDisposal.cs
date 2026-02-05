using ModulerERP.SharedKernel.Entities;
using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Domain.Entities;

public class AssetDisposal : BaseEntity
{
    public Guid AssetId { get; private set; }
    
    public DateTime DisposalDate { get; private set; }
    public DisposalType Type { get; private set; }
    
    public Guid? PartnerId { get; private set; }
    public decimal SaleAmount { get; private set; }
    public decimal BookValueAtDate { get; private set; }
    public decimal ProfitLoss { get; private set; } // SaleAmount - BookValueAtDate

    // Navigation
    public Asset Asset { get; private set; } = null!;

    private AssetDisposal() { }

    public static AssetDisposal Create(
        Guid tenantId,
        Guid assetId,
        DateTime disposalDate,
        DisposalType type,
        decimal saleAmount,
        decimal bookValueAtDate,
        Guid createdByUserId,
        Guid? partnerId = null)
    {
        var disposal = new AssetDisposal
        {
            AssetId = assetId,
            DisposalDate = disposalDate,
            Type = type,
            SaleAmount = saleAmount,
            BookValueAtDate = bookValueAtDate,
            ProfitLoss = saleAmount - bookValueAtDate,
            PartnerId = partnerId
        };
        
        disposal.SetTenant(tenantId);
        disposal.SetCreator(createdByUserId);
        
        return disposal;
    }
}
