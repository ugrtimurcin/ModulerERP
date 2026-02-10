using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProgressPayment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public int PaymentNo { get; set; } // 1, 2, 3...
    public DateTime Date { get; set; }
    
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Green Book Columns (Yeşil Defter)
    public decimal GrossWorkAmount { get; set; } // Total production value (Kumulatif İmalat)
    public decimal MaterialOnSiteAmount { get; set; } // İhzar
    
    public decimal CumulativeTotalAmount { get; set; } // Gross + MaterialOnSite
    public decimal PreviousCumulativeAmount { get; set; } // from Payment N-1
    
    public decimal PeriodDeltaAmount { get; private set; } // Current - Previous

    // Deductions
    public decimal RetentionRate { get; set; } // Teminat Oranı
    public decimal RetentionAmount { get; set; } // Teminat Kesintisi
    
    public decimal WithholdingTaxRate { get; set; } // Stopaj Oranı
    public decimal WithholdingTaxAmount { get; set; } // Stopaj Kesintisi
    
    public decimal AdvanceDeductionAmount { get; set; } // Avans Kesintisi
    
    public decimal NetPayableAmount { get; private set; }
    
    public bool IsExpense { get; set; } // True: Subcontractor, False: Client

    public ProgressPaymentStatus Status { get; set; } = ProgressPaymentStatus.Draft;

    public ICollection<ProgressPaymentDetail> Details { get; set; } = new List<ProgressPaymentDetail>();

    public void Calculate()
    {
        // Sum from Details if available
        if (Details.Any())
        {
            GrossWorkAmount = Details.Sum(x => x.TotalAmount);
        }

        CumulativeTotalAmount = GrossWorkAmount + MaterialOnSiteAmount;
        PeriodDeltaAmount = CumulativeTotalAmount - PreviousCumulativeAmount;
        
        // Deductions are usually calculated on the Delta
        if (RetentionAmount == 0 && RetentionRate > 0)
            RetentionAmount = PeriodDeltaAmount * RetentionRate;
            
        if (WithholdingTaxAmount == 0 && WithholdingTaxRate > 0)
            WithholdingTaxAmount = PeriodDeltaAmount * WithholdingTaxRate;
            
        NetPayableAmount = PeriodDeltaAmount - RetentionAmount - WithholdingTaxAmount - AdvanceDeductionAmount;
    }
}
