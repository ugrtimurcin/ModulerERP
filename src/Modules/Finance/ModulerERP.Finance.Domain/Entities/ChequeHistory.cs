using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

public class ChequeHistory : BaseEntity
{
    public Guid ChequeId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public ChequeStatus FromStatus { get; private set; }
    public ChequeStatus ToStatus { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public Cheque Cheque { get; private set; } = null!;

    private ChequeHistory() { }

    public static ChequeHistory Create(
        Guid tenantId,
        Guid chequeId,
        ChequeStatus fromStatus,
        ChequeStatus toStatus,
        string description,
        Guid createdByUserId)
    {
        var history = new ChequeHistory
        {
            ChequeId = chequeId,
            TransactionDate = DateTime.UtcNow,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Description = description
        };
        history.SetTenant(tenantId);
        history.SetCreator(createdByUserId);
        return history;
    }
}
