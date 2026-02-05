namespace ModulerERP.HR.Domain.Enums;

public enum EmploymentStatus
{
    Active = 1,
    Terminated = 2,
    Resigned = 3,
    OnLeave = 4
}

public enum AttendanceType
{
    CheckIn = 1,
    CheckOut = 2
}

public enum CommissionBasis
{
    InvoicedAmount = 1,
    CollectedAmount = 2,
    GrossProfit = 3
}

public enum PayrollStatus
{
    Draft = 1,
    Approved = 2,
    Paid = 3
}

public enum CommissionStatus
{
    Draft = 1,
    Approved = 2
}

public enum AdvanceRequestStatus
{
    Pending = 1,
    Approved = 2,
    Paid = 3,
    Deducted = 4,
    Rejected = 5
}
