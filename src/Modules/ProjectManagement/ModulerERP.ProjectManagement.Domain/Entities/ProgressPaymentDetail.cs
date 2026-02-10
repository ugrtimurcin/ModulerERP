using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProgressPaymentDetail : BaseEntity
{
    public Guid ProgressPaymentId { get; set; }
    public Guid BillOfQuantitiesItemId { get; set; }

    // Quantities
    public decimal PreviousCumulativeQuantity { get; set; } // Snapshot from previous payment
    public decimal CumulativeQuantity { get; set; } // User Input (Current Total)
    
    public decimal PeriodQuantity => CumulativeQuantity - PreviousCumulativeQuantity;

    // Snapshot of Price at moment of payment (in case contract price changes)
    public decimal UnitPrice { get; set; } 

    public decimal TotalAmount => CumulativeQuantity * UnitPrice;
    public decimal PeriodAmount => PeriodQuantity * UnitPrice;

    // Navigation
    // public ProgressPayment ProgressPayment { get; set; }
    public BillOfQuantitiesItem BillOfQuantitiesItem { get; set; }
}
