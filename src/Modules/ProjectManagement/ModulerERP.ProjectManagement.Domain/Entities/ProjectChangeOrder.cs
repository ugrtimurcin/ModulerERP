using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectChangeOrder : BaseEntity
{
    public Guid ProjectId { get; set; }
    public int OrderNo { get; set; } // Sequential: 1, 2, 3...
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Changes
    public decimal AmountChange { get; set; } // Can be negative
    public int TimeExtensionDays { get; set; } // Can be negative
    
    // Status
    public ChangeOrderStatus Status { get; set; } = ChangeOrderStatus.Draft;
    
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public Guid? ApproverId { get; set; }

    public void Approve(Guid approverId)
    {
        if (Status != ChangeOrderStatus.PendingApproval && Status != ChangeOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft or Pending changes can be approved.");

        Status = ChangeOrderStatus.Approved;
        ApprovalDate = DateTime.UtcNow;
        ApproverId = approverId;
    }

    public void Reject(Guid approverId)
    {
        if (Status != ChangeOrderStatus.PendingApproval && Status != ChangeOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft or Pending changes can be rejected.");

        Status = ChangeOrderStatus.Rejected;
        ApprovalDate = DateTime.UtcNow;
        ApproverId = approverId;
    }
}

public enum ChangeOrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3
}
