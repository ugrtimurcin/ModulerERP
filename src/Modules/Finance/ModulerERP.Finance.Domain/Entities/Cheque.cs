using ModulerERP.SharedKernel.Entities;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

public class Cheque : BaseEntity
{
    public string ChequeNumber { get; private set; } = string.Empty;
    public ChequeType Type { get; private set; }
    public string BankName { get; private set; } = string.Empty;
    public string? BranchName { get; private set; }
    public string? AccountNumber { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public Guid CurrencyId { get; private set; }

    public ChequeStatus CurrentStatus { get; private set; }
    public Guid? CurrentLocationId { get; private set; } // PartnerId, AccountId, etc.
    public string? Drawer { get; private set; } // Person who gave the cheque

    private Cheque() { } 

    public static Cheque Create(
        Guid tenantId,
        string chequeNumber,
        ChequeType type,
        string bankName,
        DateTime dueDate,
        decimal amount,
        Guid currencyId,
        string? drawer,
        Guid createdByUserId)
    {
        var cheque = new Cheque
        {
            ChequeNumber = chequeNumber,
            Type = type,
            BankName = bankName,
            DueDate = dueDate,
            Amount = amount,
            CurrencyId = currencyId,
            CurrentStatus = ChequeStatus.Portfolio,
            Drawer = drawer
        };
        cheque.SetTenant(tenantId);
        cheque.SetCreator(createdByUserId);
        return cheque;
    }

    public void UpdateStatus(ChequeStatus newStatus, Guid? newLocationId, Guid userId)
    {
        // Simple state validation
        if (CurrentStatus == ChequeStatus.Paid || CurrentStatus == ChequeStatus.Bounced)
        {
             // Allow 'Returned' from these states if necessary, but strictly usually not.
             // For now blocking moving out of terminal states.
             if (newStatus != ChequeStatus.Returned) // Example exception
                throw new InvalidOperationException($"Cannot change status of a {CurrentStatus} cheque.");
        }
        
        CurrentStatus = newStatus;
        CurrentLocationId = newLocationId;
        SetUpdater(userId);
    }
}
