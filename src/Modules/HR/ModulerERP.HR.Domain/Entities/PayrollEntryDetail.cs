using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PayrollEntryDetail : BaseEntity
{
    public Guid PayrollEntryId { get; private set; }
    public Guid EarningDeductionTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public PayrollEntry? PayrollEntry { get; private set; }
    public EarningDeductionType? Type { get; private set; }

    private PayrollEntryDetail() { }

    public static PayrollEntryDetail Create(Guid tenantId, Guid createdBy, Guid payrollEntryId, Guid typeId, decimal amount, string? description)
    {
        var detail = new PayrollEntryDetail
        {
            PayrollEntryId = payrollEntryId,
            EarningDeductionTypeId = typeId,
            Amount = amount,
            Description = description
        };
        detail.SetTenant(tenantId);
        detail.SetCreator(createdBy);
        return detail;
    }
}
