using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PublicHoliday : BaseEntity
{
    public DateTime Date { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsHalfDay { get; private set; }

    private PublicHoliday() { }

    public static PublicHoliday Create(Guid tenantId, Guid createdBy, DateTime date, string name, bool isHalfDay = false)
    {
        var ph = new PublicHoliday
        {
            Date = date,
            Name = name,
            IsHalfDay = isHalfDay
        };
        ph.SetTenant(tenantId);
        ph.SetCreator(createdBy);
        return ph;
    }
}
