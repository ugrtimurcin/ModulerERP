using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class SalaryHistory : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime ChangeDate { get; private set; }
    public decimal OldSalary { get; private set; }
    public decimal NewSalary { get; private set; }
    public Guid? NewCurrencyId { get; private set; }
    public string? Reason { get; private set; }

    public Employee? Employee { get; private set; }

    private SalaryHistory() { }

    public static SalaryHistory Create(Guid tenantId, Guid createdBy, Guid employeeId, decimal oldSal, decimal newSal, Guid? newCurrId, string? reason)
    {
        var item = new SalaryHistory
        {
            EmployeeId = employeeId,
            ChangeDate = DateTime.UtcNow,
            OldSalary = oldSal,
            NewSalary = newSal,
            NewCurrencyId = newCurrId,
            Reason = reason
        };
        item.SetTenant(tenantId);
        item.SetCreator(createdBy);
        return item;
    }
}
