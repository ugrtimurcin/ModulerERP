using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class PayrollEntry : BaseEntity
{
    public Guid PayrollId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    
    // YTD Tracking
    public decimal CumulativeTaxBaseBeforeThisPayroll { get; private set; }
    
    // Aggregated Earning/Deduction Totals
    public decimal TotalTaxableEarnings { get; private set; }
    public decimal TotalSgkExemptEarnings { get; private set; }
    
    // Deductions (Employee)
    public decimal SocialSecurityEmployee { get; private set; } // 9%
    public decimal ProvidentFundEmployee { get; private set; } // 5%
    public decimal UnemploymentInsuranceEmployee { get; private set; }
    public decimal PersonalAllowanceDeduction { get; private set; } // Applied before Income Tax
    public decimal IncomeTax { get; private set; }
    public decimal StampTax { get; private set; } // Damga Vergisi
    
    // Employer Costs
    public decimal SocialSecurityEmployer { get; private set; }
    public decimal ProvidentFundEmployer { get; private set; }
    public decimal UnemploymentInsuranceEmployer { get; private set; }

    public decimal NetPayable { get; private set; }
    public decimal ExchangeRate { get; private set; }

    public ICollection<PayrollEntryDetail> Details { get; private set; }

    public Payroll? Payroll { get; private set; }
    public Employee? Employee { get; private set; }

    private PayrollEntry() 
    {
        Details = new List<PayrollEntryDetail>();
    }

    public static PayrollEntry Create(
        Guid tenantId,
        Guid createdBy,
        Guid payrollId,
        Guid employeeId,
        decimal baseSalary,
        decimal cumulativeTaxBaseBeforeThisPayroll,
        decimal totalTaxableEarnings,
        decimal totalSgkExemptEarnings,
        decimal socialSecurityEmp,
        decimal providentFundEmp,
        decimal unemploymentInsEmp,
        decimal personalAllowanceDeduction,
        decimal incomeTax,
        decimal stampTax,
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
            CumulativeTaxBaseBeforeThisPayroll = cumulativeTaxBaseBeforeThisPayroll,
            TotalTaxableEarnings = totalTaxableEarnings,
            TotalSgkExemptEarnings = totalSgkExemptEarnings,
            SocialSecurityEmployee = socialSecurityEmp,
            ProvidentFundEmployee = providentFundEmp,
            UnemploymentInsuranceEmployee = unemploymentInsEmp,
            PersonalAllowanceDeduction = personalAllowanceDeduction,
            IncomeTax = incomeTax,
            StampTax = stampTax,
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
