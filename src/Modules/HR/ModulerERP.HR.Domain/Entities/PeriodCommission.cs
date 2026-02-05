using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class PeriodCommission : BaseEntity
{
    public string Period { get; private set; } = string.Empty; // '2026-01'
    public Guid EmployeeId { get; private set; }
    public decimal CalculatedAmount { get; private set; }
    public decimal FinalAmount { get; private set; } // Manager Approved
    public CommissionStatus Status { get; private set; } = CommissionStatus.Draft;

    public Employee? Employee { get; private set; }

    private PeriodCommission() { }

    public static PeriodCommission Create(Guid tenantId, Guid createdBy, Guid employeeId, string period, decimal amount)
    {
        var pc = new PeriodCommission
        {
            EmployeeId = employeeId,
            Period = period,
            CalculatedAmount = amount,
            FinalAmount = amount // Default to calc
        };
        pc.SetTenant(tenantId);
        pc.SetCreator(createdBy);
        return pc;
    }
}
