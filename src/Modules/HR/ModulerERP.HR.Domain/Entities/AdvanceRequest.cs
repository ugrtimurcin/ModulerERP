using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class AdvanceRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime RequestDate { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? RepaymentDate { get; private set; }
    public bool IsPaid { get; private set; } // Paid to employee
    public bool IsDeducted { get; private set; } // Deducted from payroll
    public AdvanceRequestStatus Status { get; private set; } = AdvanceRequestStatus.Pending;

    public Employee? Employee { get; private set; }

    private AdvanceRequest() { }

    public string? Description { get; private set; }

    public static AdvanceRequest Create(Guid tenantId, Guid createdBy, Guid employeeId, decimal amount, DateTime? repaymentDate, string? description = null)
    {
        var ar = new AdvanceRequest
        {
            EmployeeId = employeeId,
            RequestDate = DateTime.UtcNow,
            Amount = amount,
            RepaymentDate = repaymentDate,
            Description = description,
            Status = AdvanceRequestStatus.Pending,
            IsPaid = false,
            IsDeducted = false
        };
        ar.SetTenant(tenantId);
        ar.SetCreator(createdBy);
        return ar;
    }

    public void SetStatus(AdvanceRequestStatus status)
    {
        Status = status;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
    }

    public void MarkAsDeducted()
    {
        IsDeducted = true;
    }
}
