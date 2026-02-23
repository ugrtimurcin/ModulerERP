using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class LeaveAllocation : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid LeavePolicyId { get; private set; }
    public int Year { get; private set; }
    public int TotalDaysAllocated { get; private set; }
    public int DaysUsed { get; private set; }
    public string? AllocationReason { get; private set; } // e.g., "Annual Grant", "Brought Forward"

    public int DaysRemaining => TotalDaysAllocated - DaysUsed;
    
    public Employee? Employee { get; private set; }
    public LeavePolicy? LeavePolicy { get; private set; }

    private LeaveAllocation() { }

    public static LeaveAllocation Create(Guid tenantId, Guid createdBy, Guid employeeId, Guid leavePolicyId, int year, int initialDays, string? reason = null)
    {
        var allocation = new LeaveAllocation
        {
            EmployeeId = employeeId,
            LeavePolicyId = leavePolicyId,
            Year = year,
            TotalDaysAllocated = initialDays,
            DaysUsed = 0,
            AllocationReason = reason
        };
        allocation.SetTenant(tenantId);
        allocation.SetCreator(createdBy);
        return allocation;
    }

    public void AddDays(int days)
    {
        TotalDaysAllocated += days;
    }

    public void UseDays(int days)
    {
        if (DaysRemaining < days)
            throw new InvalidOperationException($"Insufficient leave balance. Remaining: {DaysRemaining}, Requested: {days}");
        
        DaysUsed += days;
    }
    
    public void RestoreDays(int days)
    {
        DaysUsed -= days;
        if (DaysUsed < 0) DaysUsed = 0;
    }
}
