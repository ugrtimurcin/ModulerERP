namespace ModulerERP.ProjectManagement.Domain.Enums;

public enum ProjectTransactionSourceType
{
    DirectPurchase = 0,
    StockUsage = 1,
    LaborCost = 2,
    SubcontractorBill = 3,
    ExpenseClaim = 4,   
    Other = 99
}
