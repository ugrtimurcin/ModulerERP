namespace ModulerERP.Finance.Domain.Enums;

public enum ChequeType
{
    OwnCheque = 1,
    CustomerCheque = 2,
    Senet = 3
}

public enum ChequeStatus
{
    Portfolio = 0,      // In Hand
    Endorsed = 1,       // Given to Supplier
    BankCollection = 2, // At Bank
    Pledged = 3,        // Collateral
    Paid = 4,           // Completed
    Bounced = 5,        // Dishonored
    Returned = 6        // Sent back
}
