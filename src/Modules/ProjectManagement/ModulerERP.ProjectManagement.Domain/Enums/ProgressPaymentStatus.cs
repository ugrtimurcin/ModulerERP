namespace ModulerERP.ProjectManagement.Domain.Enums;

public enum ProgressPaymentStatus
{
    Draft = 0,
    PendingApproval = 1, // PM Approval
    PendingFinance = 2, // Finance Approval
    Approved = 3,
    Rejected = 4,
    Paid = 5
}
