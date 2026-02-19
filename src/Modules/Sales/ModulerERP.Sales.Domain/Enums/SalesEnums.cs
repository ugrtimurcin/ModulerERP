namespace ModulerERP.Sales.Domain.Enums;

/// <summary>
/// Quote status
/// </summary>
public enum QuoteStatus
{
    Draft = 0,
    Sent = 1,
    Accepted = 2,
    Rejected = 3,
    Expired = 4,
    Converted = 5
}

/// <summary>
/// Order status
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    PartiallyShipped = 2,
    Shipped = 3,
    Invoiced = 4,
    Cancelled = 5
}

/// <summary>
/// Invoice status
/// </summary>
public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

/// <summary>
/// Shipment status
/// </summary>
public enum ShipmentStatus
{
    Pending = 0,
    Shipped = 1,
    InTransit = 2,
    Delivered = 3,
    Failed = 4
}

/// <summary>
/// Return status
/// </summary>
public enum ReturnStatus
{
    Pending = 0,
    Approved = 1,
    Received = 2,
    Refunded = 3,
    Rejected = 4
}

/// <summary>
/// Credit note status
/// </summary>
public enum CreditNoteStatus
{
    Draft = 0,
    Issued = 1,
    Cancelled = 2
}
