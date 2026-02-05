using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PayrollEntry : BaseEntity
{
    public Guid PayrollId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public decimal OvertimePay { get; private set; }
    public decimal CommissionPay { get; private set; }
    public decimal AdvanceDeduction { get; private set; }
    public decimal TaxDeduction { get; private set; }
    public decimal NetPayable { get; private set; }
    public decimal ExchangeRate { get; private set; }

    public Payroll? Payroll { get; private set; }
    public Employee? Employee { get; private set; }

    private PayrollEntry() { }

    public static PayrollEntry Create(
        Guid tenantId,
        Guid createdBy,
        Guid payrollId,
        Guid employeeId,
        decimal baseSalary,
        decimal overtime,
        decimal commission,
        decimal advance,
        decimal tax,
        decimal net,
        decimal rate)
    {
        var pe = new PayrollEntry
        {
            PayrollId = payrollId,
            EmployeeId = employeeId,
            BaseSalary = baseSalary,
            OvertimePay = overtime,
            CommissionPay = commission,
            AdvanceDeduction = advance,
            TaxDeduction = tax,
            NetPayable = net,
            ExchangeRate = rate
        };
        pe.SetTenant(tenantId);
        pe.SetCreator(createdBy);
        return pe;
    }
}
