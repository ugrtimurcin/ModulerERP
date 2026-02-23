using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
namespace ModulerERP.HR.Infrastructure.Persistence.Seeding;

public static class HRSeeder
{
    private static readonly Guid SystemTenantId = new("00000000-0000-0000-0001-000000000000");
    public static IEnumerable<LeavePolicy> GetLeavePolicies()
    {
        return new[]
        {
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Annual Leave", true, false, 14),
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Sick Leave", true, true, 0),
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Maternity Leave", true, false, 112), // 16 weeks
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Paternity Leave", true, false, 3),
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Bereavement Leave", true, false, 3),
            LeavePolicy.Create(SystemTenantId, Guid.Empty, "Unpaid Leave", false, true, 0)
        };
    }

    public static IEnumerable<EarningDeductionType> GetEarningDeductionTypes()
    {
        return new[]
        {
            EarningDeductionType.Create(SystemTenantId, Guid.Empty, "Overtime (1.5x)", EarningDeductionCategory.Earning, true, true, 0),
            EarningDeductionType.Create(SystemTenantId, Guid.Empty, "Overtime (2.0x)", EarningDeductionCategory.Earning, true, true, 0),
            EarningDeductionType.Create(SystemTenantId, Guid.Empty, "Food Allowance", EarningDeductionCategory.Earning, true, false, 500m), // Exempt up to 500
            EarningDeductionType.Create(SystemTenantId, Guid.Empty, "Advance Deduction", EarningDeductionCategory.Deduction, false, false, 0),
            EarningDeductionType.Create(SystemTenantId, Guid.Empty, "Performance Bonus", EarningDeductionCategory.Earning, true, true, 0)
        };
    }

    public static IEnumerable<SgkRiskProfile> GetSgkRiskProfiles()
    {
        return new[]
        {
            SgkRiskProfile.Create(SystemTenantId, Guid.Empty, "Class 1 (Low Risk - Office)", 0.09m, "Office workers and similar low risk roles"),
            SgkRiskProfile.Create(SystemTenantId, Guid.Empty, "Class 2 (Medium Risk - Mfg)", 0.11m, "Manufacturing and similar medium risk roles"),
            SgkRiskProfile.Create(SystemTenantId, Guid.Empty, "Class 3 (High Risk - Const)", 0.13m, "Construction and similar high risk roles")
        };
    }

    public static IEnumerable<HrSetting> GetHrSettings()
    {
        var settings = new List<HrSetting>
        {
            HrSetting.Create(SystemTenantId, Guid.Empty, "WorkDaysPerWeek", "5", "Standard days worked per week"),
            HrSetting.Create(SystemTenantId, Guid.Empty, "DailyWorkHours", "8", "Standard hours worked per day"),
            HrSetting.Create(SystemTenantId, Guid.Empty, "MinimumWageNet", "35180", "Current net minimum wage"),
            HrSetting.Create(SystemTenantId, Guid.Empty, "MinimumWageGross", "40436", "Current gross minimum wage"),
            HrSetting.Create(SystemTenantId, Guid.Empty, "PersonalAllowanceFactor", "0.1", "Personal Allowance Multiplier") // 10%
        };
        return settings;
    }
}
