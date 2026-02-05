namespace ModulerERP.Procurement.Domain.Enums;

/// <summary>
/// Purchase requisition status
/// </summary>
public enum RequisitionStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Converted = 4
}

/// <summary>
/// Purchase order status
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Sent = 1,
    Confirmed = 2,
    PartiallyReceived = 3,
    FullyReceived = 4,
    Cancelled = 5
}

/// <summary>
/// Goods receipt status
/// </summary>
public enum ReceiptStatus
{
    Pending = 0,
    Received = 1,
    QualityChecked = 2
}

/// <summary>
/// Supply type for requisitions
/// </summary>
public enum SupplyType
{
    Standard = 1,
    Urgent = 2,
    ProjectBased = 3
}

/// <summary>
/// Status of Request for Quotation
/// </summary>
public enum RfqStatus
{
    Open = 1,
    Closed = 2,
    Awarded = 3
}

/// <summary>
/// Status of Supplier Quote
/// </summary>
public enum PurchaseQuoteStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}

/// <summary>
/// Status of QC Inspection
/// </summary>
public enum QualityControlStatus
{
    Pending = 1,
    Passed = 2,
    Failed = 3
}

/// <summary>
/// Status of Purchase Return
/// </summary>
public enum PurchaseReturnStatus
{
    Draft = 1,
    Shipped = 2,
    Completed = 3
}
