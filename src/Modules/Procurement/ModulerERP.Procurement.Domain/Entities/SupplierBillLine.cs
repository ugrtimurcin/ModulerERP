namespace ModulerERP.Procurement.Domain.Entities;

/// <summary>
/// Supplier bill line items.
/// </summary>
public class SupplierBillLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid SupplierBillId { get; private set; }
    public Guid? PurchaseOrderLineId { get; private set; }
    public Guid ProductId { get; private set; }
    
    public Guid? ProjectId { get; private set; }
    public Guid? ProjectTaskId { get; private set; }

    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    // Navigation
    public SupplierBill? SupplierBill { get; private set; }

    private SupplierBillLine() { } // EF Core

    public static SupplierBillLine Create(
        Guid supplierBillId,
        Guid productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxPercent = 0,
        Guid? purchaseOrderLineId = null,
        Guid? projectId = null,
        Guid? projectTaskId = null)
    {
        var netTotal = quantity * unitPrice;
        var taxAmount = netTotal * (taxPercent / 100);

        return new SupplierBillLine
        {
            SupplierBillId = supplierBillId,
            ProductId = productId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxPercent = taxPercent,
            TaxAmount = taxAmount,
            LineTotal = netTotal + taxAmount,
            PurchaseOrderLineId = purchaseOrderLineId,
            ProjectId = projectId,
            ProjectTaskId = projectTaskId
        };
    }
}
