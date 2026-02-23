using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class LeavePolicy : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsPaid { get; private set; }
    public bool RequiresSgkMissingDayCode { get; private set; }
    public int DefaultDays { get; private set; }
    public bool IsActive { get; private set; } = true;

    private LeavePolicy() { }

    public static LeavePolicy Create(Guid tenantId, Guid createdBy, string name, bool isPaid, bool requiresSgkMissingDayCode, int defaultDays)
    {
        var policy = new LeavePolicy
        {
            Name = name,
            IsPaid = isPaid,
            RequiresSgkMissingDayCode = requiresSgkMissingDayCode,
            DefaultDays = defaultDays
        };
        policy.SetTenant(tenantId);
        policy.SetCreator(createdBy);
        return policy;
    }

    public void Update(string name, bool isPaid, bool requiresSgkMissingDayCode, int defaultDays, bool isActive)
    {
        Name = name;
        IsPaid = isPaid;
        RequiresSgkMissingDayCode = requiresSgkMissingDayCode;
        DefaultDays = defaultDays;
        IsActive = isActive;
    }
}
