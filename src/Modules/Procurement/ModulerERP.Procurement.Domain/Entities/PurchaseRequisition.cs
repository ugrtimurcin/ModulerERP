using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Internal request to purchase goods/services.
/// Enables approval workflows before PO creation.
/// </summary>
public class PurchaseRequisition : BaseEntity
{
    /// <summary>Requisition number (e.g., 'REQ-2026-001')</summary>
    public string RequisitionNumber { get; private set; } = string.Empty;
    
    /// <summary>Requester - the user who needs the items</summary>
    public Guid RequesterId { get; private set; }
    
    /// <summary>Department making the request</summary>
    public Guid? DepartmentId { get; private set; }
    
    public RequisitionStatus Status { get; private set; } = RequisitionStatus.Draft;
    public SupplyType SupplyType { get; private set; } = SupplyType.Standard;
    
    /// <summary>When the items are needed</summary>
    public DateTime? RequiredDate { get; private set; }
    
    /// <summary>Business justification</summary>
    public string? Justification { get; private set; }
    
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    
    public string? Notes { get; private set; }

    // Navigation
    public ICollection<PurchaseRequisitionLine> Lines { get; private set; } = new List<PurchaseRequisitionLine>();

    private PurchaseRequisition() { } // EF Core

    public static PurchaseRequisition Create(
        Guid tenantId,
        string requisitionNumber,
        Guid requesterId,
        Guid createdByUserId,
        Guid? departmentId = null,
        SupplyType supplyType = SupplyType.Standard,
        DateTime? requiredDate = null,
        string? justification = null)
    {
        var requisition = new PurchaseRequisition
        {
            RequisitionNumber = requisitionNumber,
            RequesterId = requesterId,
            DepartmentId = departmentId,
            SupplyType = supplyType,
            RequiredDate = requiredDate,
            Justification = justification
        };

        requisition.SetTenant(tenantId);
        requisition.SetCreator(createdByUserId);
        return requisition;
    }

    public void Submit()
    {
        Status = RequisitionStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId)
    {
        Status = RequisitionStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
    }

    public void Reject() => Status = RequisitionStatus.Rejected;
    public void MarkConverted() => Status = RequisitionStatus.Converted;
}
