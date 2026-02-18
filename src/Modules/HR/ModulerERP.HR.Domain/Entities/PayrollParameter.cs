using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PayrollParameter : BaseEntity
{
    public string Key { get; private set; } = string.Empty; // e.g. "PersonalAllowanceMultiplier"
    public decimal Value { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private PayrollParameter() { }

    public static PayrollParameter Create(Guid tenantId, Guid createdBy, string key, decimal value, string description)
    {
        var p = new PayrollParameter
        {
            Key = key,
            Value = value,
            Description = description
        };
        p.SetTenant(tenantId);
        p.SetCreator(createdBy);
        return p;
    }

    public void Update(decimal value, string description)
    {
        Value = value;
        Description = description;
    }
}
