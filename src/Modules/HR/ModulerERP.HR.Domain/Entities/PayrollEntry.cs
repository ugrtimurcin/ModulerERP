using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PayrollEntry : BaseEntity
{
    public Guid PayrollId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public decimal OvertimePay { get; private set; }
    public decimal CommissionPay { get; private set; }
    public decimal Bonus { get; private set; }
    public decimal TransportationAllowance { get; private set; }
    
    // Deductions (Employee)
    public decimal SocialSecurityEmployee { get; private set; } // 9%
    public decimal ProvidentFundEmployee { get; private set; } // 5%
    public decimal UnemploymentInsuranceEmployee { get; private set; }
    public decimal IncomeTax { get; private set; }
    public decimal AdvanceDeduction { get; private set; }
    
    // Employer Costs
    public decimal SocialSecurityEmployer { get; private set; }
    public decimal ProvidentFundEmployer { get; private set; }
    public decimal UnemploymentInsuranceEmployer { get; private set; }

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
        decimal bonus,
        decimal transport,
        decimal socialSecurityEmp,
        decimal providentFundEmp,
        decimal unemploymentInsEmp,
        decimal incomeTax,
        decimal advance,
        decimal socialSecurityEmplr,
        decimal providentFundEmplr,
        decimal unemploymentInsEmplr,
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
            Bonus = bonus,
            TransportationAllowance = transport,
            SocialSecurityEmployee = socialSecurityEmp,
            ProvidentFundEmployee = providentFundEmp,
            UnemploymentInsuranceEmployee = unemploymentInsEmp,
            IncomeTax = incomeTax,
            AdvanceDeduction = advance,
            SocialSecurityEmployer = socialSecurityEmplr,
            ProvidentFundEmployer = providentFundEmplr,
            UnemploymentInsuranceEmployer = unemploymentInsEmplr,
            NetPayable = net,
            ExchangeRate = rate
        };
        pe.SetTenant(tenantId);
        pe.SetCreator(createdBy);
        return pe;
    }
}
