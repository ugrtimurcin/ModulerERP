namespace ModulerERP.ProjectManagement.Application.DTOs;

public class UpdateProgressPaymentDto
{
    public DateTime Date { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal MaterialOnSiteAmount { get; set; }
    public decimal AdvanceDeductionAmount { get; set; }
    public bool IsExpense { get; set; }
}
