using FluentAssertions;
using ModulerERP.HR.Application.Services;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using Xunit;

namespace ModulerERP.HR.Tests.Services;

public class PayrollCalculatorTests
{
    [Fact]
    public void Calculate_ShouldComputeNetSalary_ForStandardEmployee_RegressionTest()
    {
        // Arrange
        var employee = Employee.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "John", 
            "Doe", 
            "john@example.com", 
            Guid.NewGuid(), 
            "Standard", 
            "123456", 
            50000, 
            0,
            null);

        decimal gross = 50000;
        decimal bonus = 0;
        decimal overtime = 0;
        decimal commission = 0;
        decimal advance = 0;
        decimal transport = 0;

        // Dummy Tax Rules
        var taxRules = new List<TaxRule>
        {
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Bracket 1", 0, 30000, 0.10m, 1, DateTime.Now.AddYears(-1)),
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Bracket 2", 30000, decimal.MaxValue, 0.20m, 2, DateTime.Now.AddYears(-1))
        };

        // Standard Rule (9% SS, 5% PF)
        var ssRule = SocialSecurityRule.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Standard TRNC", 
            CitizenshipType.TRNC,
            SocialSecurityType.Standard,
            0.09m, 
            0.105m, 
            0.05m, 
            0.05m, 
            0.0m, 
            0.0m, 
            DateTime.Now.AddYears(-1)
        );

        // Act
        // Passing null for MinWage and Parameters to simulate legacy behavior (no ceiling, no allowances)
        var result = PayrollCalculator.Calculate(
            employee, 
            gross, 
            bonus, 
            overtime, 
            commission, 
            advance,
            transport,
            taxRules,
            ssRule,
            null,
            new List<PayrollParameter>());

        // Assert
        // TotalGross = 50000
        // Employee PF (5%) = 2500
        // Employee SS (9%) = 4500
        // Total SS Deductions = 7000
        // Tax Base = 43000
        
        // Tax:
        // 0-30000 @ 10% = 3000
        // 30000-43000 @ 20% = 2600
        // Total Tax = 5600
        
        // Net = 50000 - 7000 - 5600 = 37400
        
        result.GrossSalary.Should().Be(50000);
        result.ProvidentFundEmployee.Should().Be(2500);
        result.SocialSecurityEmployee.Should().Be(4500);
        result.IncomeTax.Should().Be(5600);
        result.NetPayable.Should().Be(37400);
    }

    [Fact]
    public void Calculate_ShouldApplyMinWageCeiling_And_TransportAllowance()
    {
        // Arrange
        var employee = Employee.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "High Earner", 
            "Boss", 
            "boss@example.com", 
            Guid.NewGuid(), 
            "Ceiling Test", 
            "999999", 
            300000, // Very high salary
            0,
            null);
        
        decimal gross = 300000;
        decimal transport = 5000; // Non-taxable allowance
        
        // Tax Rules
        var taxRules = new List<TaxRule>
        {
            TaxRule.Create(Guid.NewGuid(), Guid.NewGuid(), "Flat", 0, 0, 0.10m, 1, DateTime.Now) // Flat 10% for simplicity
        };

        // SS Rule (10% total for easy math)
        var ssRule = SocialSecurityRule.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Simple", CitizenshipType.TRNC, SocialSecurityType.Standard,
            0.10m, 0, 0, 0, 0, 0, DateTime.Now
        );

        // Minimum Wage
        var minWage = MinimumWage.Create(Guid.NewGuid(), Guid.NewGuid(), 20000, DateTime.Now);
        // Ceiling = 7 * 20000 = 140000

        // Act
        var result = PayrollCalculator.Calculate(
            employee, gross, 0, 0, 0, 0, transport,
            taxRules, ssRule, minWage, new List<PayrollParameter>()
        );

        // Assert
        // Ceiling Base = 140000 (since 300,000 > 140,000)
        // SS Deduction = 140000 * 10% = 14000
        
        // Tax Base = Gross (300000) - SS (14000) = 286000 (Note: Tax is on actual income minus deductions, usually)
        // BUT Tax Base calculation in Calculator uses result of SS deduction.
        // Tax Base = 300000 - 14000 = 286000
        // Tax = 286000 * 10% = 28600
        
        // Total Deductions = 14000 + 28600 = 42600
        
        // Net Pay = Gross (300000) - Deductions (42600) + Transport (5000)
        // Net = 262400
        
        result.SocialSecurityEmployee.Should().Be(14000); // Capped
        result.IncomeTax.Should().Be(28600);
        result.TransportAmount.Should().Be(5000);
        result.NetPayable.Should().Be(262400);
    }
}
