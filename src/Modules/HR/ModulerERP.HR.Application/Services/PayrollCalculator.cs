using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.Services;

public static class PayrollCalculator
{
    // KKTC 2026 Tax Brackets (Example - to be refined with actual data)
    // Cumulative Income Tax
    private static readonly (decimal Limit, decimal Rate)[] TaxBrackets = 
    {
        (30000, 0.10m),
        (60000, 0.20m),
        (100000, 0.25m),
        (decimal.MaxValue, 0.37m)
    };

    public static PayrollCalculationResult Calculate(Employee employee, decimal grossSalary, decimal bonus, decimal overtime, decimal commission, decimal advance)
    {
        var totalGross = grossSalary + bonus + overtime + commission;
        
        // Social Security (Ihtiyat Sandigi)
        // Employee: 5% Provident, 9% Social Security
        var providentFundEmp = totalGross * 0.05m;
        var socialSecurityEmp = totalGross * 0.09m;
        
        // Employer: 5% Provident, 10.5% Social Security, Unemployment?
        var providentFundEmplr = totalGross * 0.05m;
        var socialSecurityEmplr = totalGross * 0.105m;
        var unemploymentIns = totalGross * 0.0m; // Placeholder

        var taxableIncome = totalGross - (providentFundEmp + socialSecurityEmp);
        
        // Income Tax Calculation
        var incomeTax = CalculateIncomeTax(taxableIncome);

        var totalDeductions = providentFundEmp + socialSecurityEmp + incomeTax + advance;
        var netPayable = totalGross - totalDeductions;

        return new PayrollCalculationResult
        {
            GrossSalary = totalGross,
            SocialSecurityEmployee = socialSecurityEmp,
            ProvidentFundEmployee = providentFundEmp,
            IncomeTax = incomeTax,
            AdvanceDeduction = advance,
            SocialSecurityEmployer = socialSecurityEmplr,
            ProvidentFundEmployer = providentFundEmplr,
            UnemploymentInsuranceEmployer = unemploymentIns,
            NetPayable = netPayable
        };
    }

    private static decimal CalculateIncomeTax(decimal taxableIncome)
    {
        decimal tax = 0;
        decimal previousLimit = 0;

        foreach (var bracket in TaxBrackets)
        {
            if (taxableIncome > previousLimit)
            {
                var taxableAmount = Math.Min(taxableIncome, bracket.Limit) - previousLimit;
                tax += taxableAmount * bracket.Rate;
                previousLimit = bracket.Limit;
            }
            else
            {
                break;
            }
        }
        return tax;
    }
}

public class PayrollCalculationResult
{
    public decimal GrossSalary { get; set; }
    public decimal SocialSecurityEmployee { get; set; }
    public decimal ProvidentFundEmployee { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal SocialSecurityEmployer { get; set; }
    public decimal ProvidentFundEmployer { get; set; }
    public decimal UnemploymentInsuranceEmployer { get; set; }
    public decimal NetPayable { get; set; }
}
