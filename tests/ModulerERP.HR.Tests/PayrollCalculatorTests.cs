using FluentAssertions;
using ModulerERP.HR.Application.Services;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using Xunit;

namespace ModulerERP.HR.Tests;

public class PayrollCalculatorTests
{
    private readonly MinimumWage _minWage;
    private readonly List<TaxRule> _taxRules;
    private readonly SocialSecurityRule _ssRule;
    private readonly List<PayrollParameter> _parameters;

    public PayrollCalculatorTests()
    {
        // Setup default reference data
        _minWage = MinimumWage.Create(Guid.NewGuid(), Guid.NewGuid(), 20000, DateTime.Today.AddMonths(-1)); // Min Wage = 20,000

        _taxRules = new List<TaxRule>
        {
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Bracket 1", 0, 30000, 0.10m, 1, DateTime.Today),
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Bracket 2", 30000, 60000, 0.20m, 2, DateTime.Today),
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Bracket 3", 60000, 0, 0.30m, 3, DateTime.Today)
        };

        _ssRule = SocialSecurityRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Standard", 
            CitizenshipType.TRNC,
            SocialSecurityType.Standard,
            0.04m, // SS Emp
            0.04m, // SS Emplr
            0.05m, // PF Emp
            0.05m, // PF Emplr
            0.01m, // UI Emp
            0.01m, // UI Emplr
            DateTime.Today
        );

        _parameters = new List<PayrollParameter>
        {
            PayrollParameter.Create(Guid.NewGuid(), Guid.NewGuid(), "PersonalAllowanceMultiplier", 1.0m, "Multiplier for Personal Allowance"),
            PayrollParameter.Create(Guid.NewGuid(), Guid.NewGuid(), "SpouseAllowanceMultiplier", 0.5m, "Multiplier for Spouse Allowance"),
            PayrollParameter.Create(Guid.NewGuid(), Guid.NewGuid(), "ChildAllowanceMultiplier", 0.1m, "Multiplier for Child Allowance")
        };
    }

    [Fact]
    public void Calculate_StandardEmployee_ShouldCalculateCorrectly()
    {
        // Arrange
        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), "John", "Doe", "john@example.com", Guid.NewGuid(), "Dev", "123", 
            salary: 40000, 
            transportAmount: 0);

        decimal gross = 40000;
        
        // Act
        var result = PayrollCalculator.Calculate(employee, gross, 0, 0, 0, 0, 0, _taxRules, _ssRule, _minWage, _parameters);

        // Assert
        // SS Basis = 40,000 (MinWage 20k, Ceiling 140k. 40k < 140k)
        // SS Deductions (Emp): 5% + 4% + 1% = 10% = 4,000
        result.SocialSecurityEmployee.Should().Be(1600); // 4%
        result.ProvidentFundEmployee.Should().Be(2000);  // 5%
        result.UnemploymentInsuranceEmployee.Should().Be(400); // 1%
        
        // Tax Base = Gross - SS = 40,000 - 4,000 = 36,000
        // Personal Allowance = 1.0 * 20,000 = 20,000
        // Taxable Income = 36,000 - 20,000 = 16,000
        
        // Tax Calculation:
        // Bracket 1 (0-30k) @ 10%: 16,000 * 0.10 = 1,600
        result.IncomeTax.Should().Be(1600);

        // Net Pay = Gross - SS - Tax = 40,000 - 4,000 - 1,600 = 34,400
        result.NetPayable.Should().Be(34400);
    }

    [Fact]
    public void Calculate_MarriedWithChildren_ShouldHaveLowerTax()
    {
        // Arrange
        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), "Jane", "Doe", "jane@example.com", Guid.NewGuid(), "Manager", "124", 
            salary: 40000, 
            transportAmount: 0,
            maritalStatus: MaritalStatus.Married,
            isSpouseWorking: false,
            childCount: 2);

        decimal gross = 40000;
        
        // Act
        var result = PayrollCalculator.Calculate(employee, gross, 0, 0, 0, 0, 0, _taxRules, _ssRule, _minWage, _parameters);

        // Assert
        // Tax Base = 36,000
        // Personal Allowance = (1.0 + 0.5 + (0.1 * 2)) * 20,000 
        //                    = (1.7) * 20,000 = 34,000
        
        // Taxable Income = 36,000 - 34,000 = 2,000
        // Tax: 2,000 * 0.10 = 200
        
        result.IncomeTax.Should().Be(200);
        result.NetPayable.Should().Be(35800); // 40,000 - 4,000 - 200
    }

    [Fact]
    public void Calculate_HighSalary_ShouldCapSocialSecurity()
    {
        // Arrange
        decimal minWageAmount = 20000;
        decimal ceiling = minWageAmount * 7; // 140,000
        decimal gross = 200000; // Above ceiling

        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), "Rich", "Guy", "rich@example.com", Guid.NewGuid(), "CEO", "125", 
            salary: gross, 
            transportAmount: 0);

        // Act
        var result = PayrollCalculator.Calculate(employee, gross, 0, 0, 0, 0, 0, _taxRules, _ssRule, _minWage, _parameters);

        // Assert
        // SS Basis should be capped at 140,000
        // SS (Emp) 10% of 140,000 = 14,000
        var expectedProvident = ceiling * 0.05m; // 7,000
        var expectedSS = ceiling * 0.04m;        // 5,600
        var expectedUI = ceiling * 0.01m;        // 1,400

        result.ProvidentFundEmployee.Should().Be(expectedProvident);
        result.SocialSecurityEmployee.Should().Be(expectedSS);
        result.UnemploymentInsuranceEmployee.Should().Be(expectedUI);

        // Tax Base = 200,000 - 14,000 = 186,000
        // Personal Allowance = 20,000
        // Taxable = 166,000
        
        // Tax:
        // 0-30k @ 10% = 3,000
        // 30k-60k @ 20% = 6,000
        // 60k+ (106k) @ 30% = 31,800
        // Total Tax = 40,800
        
        result.IncomeTax.Should().Be(40800);
    }

    [Fact]
    public void Calculate_WithTransport_ShouldAddTransportToNetPay()
    {
        // Arrange
        decimal transport = 2000;
        var employee = Employee.Create(Guid.NewGuid(), Guid.NewGuid(), "Driver", "Fast", "driver@example.com", Guid.NewGuid(), "Driver", "126", 
            salary: 30000, 
            transportAmount: transport);

        decimal gross = 30000;

        // Act
        var result = PayrollCalculator.Calculate(employee, gross, 0, 0, 0, 0, transport, _taxRules, _ssRule, _minWage, _parameters);

        // Assert
        // Standard SS (10% of 30k) = 3,000
        // Tax Base = 27,000
        // Personal Allowance = 20,000
        // Taxable = 7,000
        // Tax = 700
        
        // Deductions = 3,700
        // Base Net = 30,000 - 3,700 = 26,300
        // Final Net = 26,300 + 2,000 = 28,300
        
        result.NetPayable.Should().Be(28300);
        result.TransportAmount.Should().Be(transport);
    }
}
