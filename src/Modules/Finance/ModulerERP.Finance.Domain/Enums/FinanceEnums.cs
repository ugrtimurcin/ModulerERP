namespace ModulerERP.Finance.Domain.Enums;

/// <summary>
/// Account types for Chart of Accounts
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

/// <summary>
/// Journal entry status
/// </summary>
public enum JournalStatus
{
    Draft = 0,
    Posted = 1,
    Voided = 2
}

/// <summary>
/// Payment direction
/// </summary>
public enum PaymentDirection
{
    Incoming = 1,  // Customer payment to us
    Outgoing = 2   // Payment to supplier
}

/// <summary>
/// Payment method
/// </summary>
public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Check = 3,
    CreditCard = 4,
    Other = 5
}

/// <summary>
/// Fiscal period status
/// </summary>
public enum PeriodStatus
{
    Open = 1,
    Closed = 2,
    Locked = 3
}

/// <summary>
/// Exchange rate source
/// </summary>
public enum ExchangeRateSource
{
    Manual = 1,
    TCMB = 2,     // Central Bank of Turkey
    ECB = 3,      // European Central Bank
    Custom = 4
}
