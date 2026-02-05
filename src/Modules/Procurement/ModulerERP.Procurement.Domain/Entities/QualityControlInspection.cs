using ModulerERP.SharedKernel.Entities;
using ModulerERP.Procurement.Domain.Enums;

namespace ModulerERP.Procurement.Domain.Entities;

public class QualityControlInspection : BaseEntity
{
    public Guid ReceiptItemId { get; private set; }
    public Guid InspectorId { get; private set; }
    public DateTime InspectionDate { get; private set; }
    public decimal QuantityPassed { get; private set; }
    public decimal QuantityRejected { get; private set; }
    public Guid? RejectionReasonId { get; private set; }
    public Guid TargetWarehouseId { get; private set; }
    public Guid? TargetLocationId { get; private set; }
    public QualityControlStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private QualityControlInspection() { }

    public static QualityControlInspection Create(
        Guid tenantId,
        Guid receiptItemId,
        Guid inspectorId,
        DateTime inspectionDate,
        decimal quantityPassed,
        decimal quantityRejected,
        Guid targetWarehouseId,
        Guid createdByUserId,
        Guid? rejectionReasonId = null,
        Guid? targetLocationId = null,
        string? notes = null)
    {
        var qc = new QualityControlInspection
        {
            ReceiptItemId = receiptItemId,
            InspectorId = inspectorId,
            InspectionDate = inspectionDate,
            QuantityPassed = quantityPassed,
            QuantityRejected = quantityRejected,
            RejectionReasonId = rejectionReasonId,
            TargetWarehouseId = targetWarehouseId,
            TargetLocationId = targetLocationId,
            Status = quantityRejected > 0 ? QualityControlStatus.Failed : QualityControlStatus.Passed,
            Notes = notes
        };
        qc.SetTenant(tenantId);
        qc.SetCreator(createdByUserId);
        return qc;
    }
}
