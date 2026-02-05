using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class AdvanceRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime RequestDate { get; private set; }
    public decimal Amount { get; private set; }
    public AdvanceRequestStatus Status { get; private set; } = AdvanceRequestStatus.Pending;

    public Employee? Employee { get; private set; }

    private AdvanceRequest() { }

    public string? Description { get; private set; }

    public static AdvanceRequest Create(Guid tenantId, Guid createdBy, Guid employeeId, decimal amount, string? description = null)
    {
        var ar = new AdvanceRequest
        {
            EmployeeId = employeeId,
            RequestDate = DateTime.UtcNow,
            Amount = amount,
            Description = description,
            Status = AdvanceRequestStatus.Pending
        };
        ar.SetTenant(tenantId);
        ar.SetCreator(createdBy);
        return ar;
    }

    public void SetStatus(AdvanceRequestStatus status)
    {
        Status = status;
    }
}
