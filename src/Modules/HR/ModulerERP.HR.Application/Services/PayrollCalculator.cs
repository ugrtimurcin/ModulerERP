using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums; // Required for MaritalStatus

namespace ModulerERP.HR.Application.Services;

public static class PayrollCalculator
{
    public static PayrollCalculationResult Calculate(
        Employee employee, 
        decimal grossSalary, 
        decimal bonus, 
        decimal overtime, 
        decimal commission, 
        decimal advance,
        decimal transportAmount, // Added
        IEnumerable<TaxRule> taxRules,
        SocialSecurityRule? socialSecurityRule,
        MinimumWage? minimumWage, // Added
        IEnumerable<PayrollParameter> parameters // Added
    )
    {
        var totalGross = grossSalary + bonus + overtime + commission;
        
        // --- Social Security Logic (TRNC) ---
        // Ceiling (Tavan) Logic: Max deduction base is 7x Minimum Wage
        decimal deductionBase = totalGross;
        if (minimumWage != null && minimumWage.GrossAmount > 0)
        {
            var ceiling = minimumWage.GrossAmount * 7;
            if (deductionBase > ceiling)
            {
                deductionBase = ceiling;
            }
        }

        // Social Security (Ihtiyat Sandigi & Sigorta & Issizlik)
        decimal providentFundEmp = 0;
        decimal socialSecurityEmp = 0;
        decimal unemploymentInsEmp = 0;
        decimal providentFundEmplr = 0;
        decimal socialSecurityEmplr = 0;
        decimal unemploymentInsEmplr = 0;
        
        if (socialSecurityRule != null)
        {
            // Apply rates to the Capped Deduction Base
            providentFundEmp = deductionBase * socialSecurityRule.ProvidentFundEmployeeRate;
            socialSecurityEmp = deductionBase * socialSecurityRule.EmployeeDeductionRate;
            unemploymentInsEmp = deductionBase * socialSecurityRule.UnemploymentInsuranceEmployeeRate;
            
            providentFundEmplr = deductionBase * socialSecurityRule.ProvidentFundEmployerRate;
            socialSecurityEmplr = deductionBase * socialSecurityRule.EmployerDeductionRate;
            unemploymentInsEmplr = deductionBase * socialSecurityRule.UnemploymentInsuranceEmployerRate;
        }

        // --- Tax Logic (TRNC) ---
        // Tax Base = Gross - SS Deductions - Personal Allowances
        var totalSSDeductions = providentFundEmp + socialSecurityEmp + unemploymentInsEmp;
        var initialTaxBase = totalGross - totalSSDeductions;
        
        // Calculate Personal Allowances
        decimal personalAllowance = 0;
        if (minimumWage != null)
        {
            // Example TRNC Logic: Personal Allowance is often % of Min Wage or fixed annual amount
            // We use PayrollParameters to define the multipliers
            var personalMultiplier = parameters.FirstOrDefault(p => p.Key == "PersonalAllowanceMultiplier")?.Value ?? 0;
            var spouseMultiplier = parameters.FirstOrDefault(p => p.Key == "SpouseAllowanceMultiplier")?.Value ?? 0;
            var childMultiplier = parameters.FirstOrDefault(p => p.Key == "ChildAllowanceMultiplier")?.Value ?? 0;

            // Base allowance on Annual Min Wage usually, but here monthly for simplicity of example
            var baseAmount = minimumWage.GrossAmount; 

            personalAllowance += baseAmount * personalMultiplier;
            
            if (employee.MaritalStatus == MaritalStatus.Married && !employee.IsSpouseWorking)
            {
                personalAllowance += baseAmount * spouseMultiplier;
            }

            if (employee.ChildCount > 0)
            {
                personalAllowance += baseAmount * childMultiplier * employee.ChildCount;
            }
        }

        var taxableIncome = initialTaxBase - personalAllowance;
        if (taxableIncome < 0) taxableIncome = 0;
        
        // Income Tax Calculation
        var incomeTax = CalculateIncomeTax(taxableIncome, taxRules);

        var totalDeductions = totalSSDeductions + incomeTax + advance;
        
        // --- Net Pay Logic ---
        // Net Pay = Gross - Deductions + Non-Taxable Allowances (Transport)
        var netPayable = totalGross - totalDeductions + transportAmount; // BUG FIX: Added transportAmount

        return new PayrollCalculationResult
        {
            GrossSalary = totalGross,
            SocialSecurityEmployee = socialSecurityEmp,
            ProvidentFundEmployee = providentFundEmp,
            UnemploymentInsuranceEmployee = unemploymentInsEmp,
            IncomeTax = incomeTax,
            AdvanceDeduction = advance,
            SocialSecurityEmployer = socialSecurityEmplr,
            ProvidentFundEmployer = providentFundEmplr,
            UnemploymentInsuranceEmployer = unemploymentInsEmplr,
            NetPayable = netPayable,
            TransportAmount = transportAmount
        };
    }

    private static decimal CalculateIncomeTax(decimal taxableIncome, IEnumerable<TaxRule> rules)
    {
        decimal tax = 0;
        decimal previousLimit = 0;

        // Ensure rules are ordered
        var orderedRules = rules.OrderBy(r => r.Order).ToList();

        foreach (var bracket in orderedRules)
        {
            if (taxableIncome > previousLimit)
            {
                var currentUpper = bracket.UpperLimit;
                // Check if last bracket (MaxValue)
                if (currentUpper == 0) currentUpper = decimal.MaxValue;

                var amountInBracket = Math.Min(taxableIncome, currentUpper) - previousLimit;
                
                if (amountInBracket > 0)
                {
                    tax += amountInBracket * bracket.Rate;
                }

                previousLimit = currentUpper;
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
    public decimal UnemploymentInsuranceEmployee { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal SocialSecurityEmployer { get; set; }
    public decimal ProvidentFundEmployer { get; set; }
    public decimal UnemploymentInsuranceEmployer { get; set; }
    public decimal NetPayable { get; set; }
    public decimal TransportAmount { get; set; }
}
