using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProgressPayment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public int PaymentNo { get; set; } // 1, 2, 3...
    public DateTime Date { get; set; }

    // Calculation
    public decimal PreviousCumulativeAmount { get; set; } // Önceki Hakediş Toplamı
    public decimal CurrentAmount { get; set; } // Bu hakediş tutarı

    // Retention (Teminat Kesintisi)
    public decimal RetentionRate { get; set; } // e.g., 10%
    public decimal RetentionAmount { get; set; } // Deducted amount

    // TRNC Specific Deductions/Additions
    public decimal MaterialOnSiteAmount { get; set; } // İhzar (Added to progress)
    public decimal AdvanceDeductionAmount { get; set; } // Avans Kesintisi (Deduction)
    public decimal TaxWithholdingAmount { get; set; } // Stopaj (Deduction from Net or Gross? Usually deduction from payable)

    // Calculation: (Current + MaterialOnSite) - (Retention + Advance + Tax)
    public decimal NetPayableAmount { get; set; }

    public ProgressPaymentStatus Status { get; set; }
}
