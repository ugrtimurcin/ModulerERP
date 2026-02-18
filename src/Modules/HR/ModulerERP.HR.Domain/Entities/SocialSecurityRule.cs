using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class SocialSecurityRule : BaseEntity
{
    public string Name { get; private set; } = string.Empty; // e.g., "Standard Employee", "Pensioner"
    public CitizenshipType CitizenshipType { get; private set; }
    public SocialSecurityType SocialSecurityType { get; private set; }
    
    public decimal EmployeeDeductionRate { get; private set; }
    public decimal EmployerDeductionRate { get; private set; }
    public decimal ProvidentFundEmployeeRate { get; private set; }
    public decimal ProvidentFundEmployerRate { get; private set; }
    public decimal UnemploymentInsuranceEmployeeRate { get; private set; }
    public decimal UnemploymentInsuranceEmployerRate { get; private set; }
    
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private SocialSecurityRule() { }

    public static SocialSecurityRule Create(
        Guid tenantId, 
        Guid createdBy, 
        string name, 
        CitizenshipType citizenshipType, 
        SocialSecurityType socialSecurityType,
        decimal employeeRate, 
        decimal employerRate, 
        decimal pfEmployeeRate, 
        decimal pfEmployerRate, 
        decimal uiEmployeeRate,
        decimal uiEmployerRate,
        DateTime effectiveFrom)
    {
        var rule = new SocialSecurityRule
        {
            Name = name,
            CitizenshipType = citizenshipType,
            SocialSecurityType = socialSecurityType,
            EmployeeDeductionRate = employeeRate,
            EmployerDeductionRate = employerRate,
            ProvidentFundEmployeeRate = pfEmployeeRate,
            ProvidentFundEmployerRate = pfEmployerRate,
            UnemploymentInsuranceEmployeeRate = uiEmployeeRate,
            UnemploymentInsuranceEmployerRate = uiEmployerRate,
            EffectiveFrom = effectiveFrom
        };
        rule.SetTenant(tenantId);
        rule.SetCreator(createdBy);
        return rule;
    }
}
