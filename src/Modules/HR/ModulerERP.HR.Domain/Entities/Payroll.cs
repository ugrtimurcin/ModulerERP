using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class Payroll : BaseEntity
{
    public string Period { get; private set; } = string.Empty; // '2026-01'
    public string Description { get; private set; } = string.Empty;
    public PayrollStatus Status { get; private set; } = PayrollStatus.Draft;
    public decimal TotalAmount { get; private set; }
    public Guid CurrencyId { get; private set; }
    public Guid? FinanceTransactionId { get; private set; } // Link to General Ledger

    public ICollection<PayrollEntry> Entries { get; private set; } = new List<PayrollEntry>();

    private Payroll() { }

    public static Payroll Create(Guid tenantId, Guid createdBy, string period, string description, Guid currencyId)
    {
        var p = new Payroll
        {
            Period = period,
            Description = description,
            CurrencyId = currencyId
        };
        p.SetTenant(tenantId);
        p.SetCreator(createdBy);
        return p;
    }

    public void AddEntry(PayrollEntry entry)
    {
        Entries.Add(entry);
        TotalAmount += entry.NetPayable;
    }
}
