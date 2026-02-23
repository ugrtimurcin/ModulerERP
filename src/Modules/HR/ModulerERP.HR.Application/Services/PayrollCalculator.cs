using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums; // Required for MaritalStatus

namespace ModulerERP.HR.Application.Services;

public static class PayrollCalculator
{
    public static PayrollCalculationResult Calculate(
        Employee employee, 
        decimal baseSalary, 
        decimal cumulativeTaxBaseBeforeThisPayroll, // NEW: Start tax calculation from here
        List<PayrollEntryDetail> earningsAndDeductions, // NEW: Generic earnings/deductions
        IEnumerable<TaxRule> taxRules,
        SgkRiskProfile? riskProfile, // NEW: Dynamic Risk Profile
        SocialSecurityRule? socialSecurityRule,
        MinimumWage? minimumWage,
        IEnumerable<PayrollParameter> parameters)
    {
        // 1. Separate generic items
        var earnings = earningsAndDeductions.Where(e => e.Type?.Category == EarningDeductionCategory.Earning).ToList();
        var fixedDeductions = earningsAndDeductions.Where(e => e.Type?.Category == EarningDeductionCategory.Deduction).ToList();
        
        var totalTaxableEarnings = earnings.Where(e => e.Type != null && e.Type.IsTaxable).Sum(e => e.Amount);
        var totalSgkExemptEarnings = earnings.Where(e => e.Type != null && e.Type.IsSgkExempt).Sum(e => e.Amount);
        var totalEarnings = earnings.Sum(e => e.Amount);
        var advanceDeduction = fixedDeductions.Sum(e => e.Amount);

        var totalGross = baseSalary + totalEarnings;
        
        // --- Social Security Logic (TRNC) ---
        // Deduction Base = Total Gross minus SGK Exempt
        decimal deductionBase = totalGross - totalSgkExemptEarnings;
        if (deductionBase < 0) deductionBase = 0;

        // Ceiling (Tavan) Logic: Max deduction base is 7x Minimum Wage
        if (minimumWage != null && minimumWage.GrossAmount > 0)
        {
            var ceiling = minimumWage.GrossAmount * 7;
            if (deductionBase > ceiling)
            {
                deductionBase = ceiling;
            }
        }

        // Social Security Deductions
        decimal providentFundEmp = 0;
        decimal socialSecurityEmp = 0;
        decimal unemploymentInsEmp = 0;
        decimal providentFundEmplr = 0;
        decimal socialSecurityEmplr = 0;
        decimal unemploymentInsEmplr = 0;
        
        if (socialSecurityRule != null)
        {
            providentFundEmp = deductionBase * socialSecurityRule.ProvidentFundEmployeeRate;
            socialSecurityEmp = deductionBase * socialSecurityRule.EmployeeDeductionRate;
            unemploymentInsEmp = deductionBase * socialSecurityRule.UnemploymentInsuranceEmployeeRate;
            
            providentFundEmplr = deductionBase * socialSecurityRule.ProvidentFundEmployerRate;
            
            // Dynamic Employer SGK based on Risk Profile
            var employerMultiplier = riskProfile?.EmployerSgkMultiplier ?? 1.00m; // Default 1.0 (no change) if not linked
            var baseEmployerRate = socialSecurityRule.EmployerDeductionRate;
            // E.g. base is 0.08, multiplier is 1.125 (representing danger class upcharge)
            socialSecurityEmplr = deductionBase * (baseEmployerRate * employerMultiplier);
            
            unemploymentInsEmplr = deductionBase * socialSecurityRule.UnemploymentInsuranceEmployerRate;
        }

        var totalSSDeductions = providentFundEmp + socialSecurityEmp + unemploymentInsEmp;

        // --- Stamp Tax (Damga Vergisi) ---
        // Applied to Total Gross in KKTC usually (say 0.00759 multiplier)
        var stampTaxRate = parameters.FirstOrDefault(p => p.Key == "StampTaxRate")?.Value ?? 0m;
        var stampTax = totalGross * stampTaxRate;


        // --- Tax Logic (TRNC) ---
        // Current Taxable = (Taxable Earnings + Base) - SGK Deductions
        var currentTaxableIncome = (baseSalary + totalTaxableEarnings) - totalSSDeductions;
        if (currentTaxableIncome < 0) currentTaxableIncome = 0;

        // Personal Allowance Breakdown
        decimal personalAllowanceDeduction = 0;
        if (minimumWage != null)
        {
            var personalMultiplier = parameters.FirstOrDefault(p => p.Key == "PersonalAllowanceMultiplier")?.Value ?? 0;
            var spouseMultiplier = parameters.FirstOrDefault(p => p.Key == "SpouseAllowanceMultiplier")?.Value ?? 0;
            var childMultiplier = parameters.FirstOrDefault(p => p.Key == "ChildAllowanceMultiplier")?.Value ?? 0;

            var baseAmount = minimumWage.GrossAmount; 

            personalAllowanceDeduction += baseAmount * personalMultiplier;
            
            if (employee.MaritalStatus == MaritalStatus.Married && !employee.IsSpouseWorking)
            {
                personalAllowanceDeduction += baseAmount * spouseMultiplier;
            }

            if (employee.ChildCount > 0)
            {
                personalAllowanceDeduction += baseAmount * childMultiplier * employee.ChildCount;
            }
        }

        // The exact taxable amount applied to the progressive brackets this month
        var strictCurrentTaxable = currentTaxableIncome - personalAllowanceDeduction;
        if (strictCurrentTaxable < 0) strictCurrentTaxable = 0;

        // NEW: Calculate Tax based on CUMULATIVE base
        var incomeTax = CalculateCumulativeIncomeTax(
            cumulativeTaxBaseBeforeThisPayroll, 
            strictCurrentTaxable, 
            taxRules);

        var totalDeductions = totalSSDeductions + stampTax + incomeTax + advanceDeduction;
        
        // --- Net Pay Logic ---
        var netPayable = totalGross - totalDeductions;

        return new PayrollCalculationResult
        {
            GrossSalary = totalGross,
            CumulativeTaxBaseBeforeThisPayroll = cumulativeTaxBaseBeforeThisPayroll,
            TotalTaxableEarnings = totalTaxableEarnings,
            TotalSgkExemptEarnings = totalSgkExemptEarnings,
            SocialSecurityEmployee = socialSecurityEmp,
            ProvidentFundEmployee = providentFundEmp,
            UnemploymentInsuranceEmployee = unemploymentInsEmp,
            PersonalAllowanceDeduction = personalAllowanceDeduction,
            IncomeTax = incomeTax,
            StampTax = stampTax,
            AdvanceDeduction = advanceDeduction,
            SocialSecurityEmployer = socialSecurityEmplr,
            ProvidentFundEmployer = providentFundEmplr,
            UnemploymentInsuranceEmployer = unemploymentInsEmplr,
            NetPayable = netPayable
        };
    }

    private static decimal CalculateCumulativeIncomeTax(decimal previousCumulative, decimal currentTaxable, IEnumerable<TaxRule> rules)
    {
        // TRNC logic: You find the tax for (Previous + Current) and subtract the tax for (Previous).
        // The difference is exactly what you owe this month, bridging brackets flawlessly.
        
        var taxOnTotal = CalculateRawTax(previousCumulative + currentTaxable, rules);
        var taxOnPrevious = CalculateRawTax(previousCumulative, rules);
        
        return taxOnTotal - taxOnPrevious;
    }

    private static decimal CalculateRawTax(decimal amount, IEnumerable<TaxRule> rules)
    {
        decimal tax = 0;
        decimal previousLimit = 0;
        var orderedRules = rules.OrderBy(r => r.Order).ToList();

        foreach (var bracket in orderedRules)
        {
            if (amount > previousLimit)
            {
                var currentUpper = bracket.UpperLimit;
                if (currentUpper == 0) currentUpper = decimal.MaxValue;

                var amountInBracket = Math.Min(amount, currentUpper) - previousLimit;
                
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
    public decimal CumulativeTaxBaseBeforeThisPayroll { get; set; }
    public decimal TotalTaxableEarnings { get; set; }
    public decimal TotalSgkExemptEarnings { get; set; }
    public decimal SocialSecurityEmployee { get; set; }
    public decimal ProvidentFundEmployee { get; set; }
    public decimal UnemploymentInsuranceEmployee { get; set; }
    public decimal PersonalAllowanceDeduction { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal StampTax { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal SocialSecurityEmployer { get; set; }
    public decimal ProvidentFundEmployer { get; set; }
    public decimal UnemploymentInsuranceEmployer { get; set; }
    public decimal NetPayable { get; set; }
}
