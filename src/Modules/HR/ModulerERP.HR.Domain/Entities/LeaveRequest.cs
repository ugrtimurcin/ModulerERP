using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid LeavePolicyId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int DaysCount { get; private set; }
    public string? Reason { get; private set; }
    public LeaveStatus Status { get; private set; } = LeaveStatus.Pending;
    public Guid? ApprovedByUserId { get; private set; }

    public Employee? Employee { get; private set; }
    public LeavePolicy? LeavePolicy { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Create(Guid tenantId, Guid createdBy, Guid employeeId, Guid leavePolicyId, DateTime start, DateTime end, int days, string? reason)
    {
        var req = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeavePolicyId = leavePolicyId,
            StartDate = start,
            EndDate = end,
            DaysCount = days,
            Reason = reason
        };
        req.SetTenant(tenantId);
        req.SetCreator(createdBy);
        return req;
    }
}



public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
