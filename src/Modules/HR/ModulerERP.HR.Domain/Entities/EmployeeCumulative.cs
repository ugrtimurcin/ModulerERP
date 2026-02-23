using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class EmployeeCumulative : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public int Year { get; private set; }
    public decimal YtdTaxBase { get; private set; } // Kümülatif Vergi Matrahı
    public decimal TotalSeveranceAccrual { get; private set; } // Accrued severance pool
    public string? PreviousEmployerNotation { get; private set; } // Audit trail if balance carried over

    public Employee? Employee { get; private set; }

    private EmployeeCumulative() { }

    public static EmployeeCumulative Create(Guid tenantId, Guid createdBy, Guid employeeId, int year, decimal startingTaxBase = 0, string? notation = null)
    {
        var cumulative = new EmployeeCumulative
        {
            EmployeeId = employeeId,
            Year = year,
            YtdTaxBase = startingTaxBase,
            PreviousEmployerNotation = notation
        };
        cumulative.SetTenant(tenantId);
        cumulative.SetCreator(createdBy);
        return cumulative;
    }

    public void AddMonthlyTaxBase(decimal monthlyTaxBase)
    {
        YtdTaxBase += monthlyTaxBase;
    }
    
    public void UpdateSeveranceAccrual(decimal accumulatedAmount)
    {
        TotalSeveranceAccrual = accumulatedAmount;
    }
}
