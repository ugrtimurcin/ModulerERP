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

    public decimal NetPayableAmount { get; set; } // CurrentAmount - Retention

    public ProgressPaymentStatus Status { get; set; }
}
