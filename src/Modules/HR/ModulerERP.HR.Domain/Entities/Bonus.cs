using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class Bonus : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } // When it was granted
    public string Period { get; private set; } = string.Empty; // e.g. "2026-02"
    public bool IsProcessed { get; private set; }

    public Employee? Employee { get; private set; }

    private Bonus() { }

    public static Bonus Create(
        Guid tenantId, 
        Guid createdBy, 
        Guid employeeId, 
        decimal amount, 
        string description, 
        DateTime date, 
        string period)
    {
        var bonus = new Bonus
        {
            EmployeeId = employeeId,
            Amount = amount,
            Description = description,
            Date = date,
            Period = period,
            IsProcessed = false
        };
        bonus.SetTenant(tenantId);
        bonus.SetCreator(createdBy);
        return bonus;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }
}
