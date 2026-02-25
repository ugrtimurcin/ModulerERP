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
    Custom = 4,
    CentralBank = 5
}

/// <summary>
/// Broad category of business events triggering ledger posts.
/// </summary>
public enum TransactionType
{
    GeneralJournal = 1,
    SalesInvoice = 2,
    PurchaseInvoice = 3,
    IncomingPayment = 4,
    OutgoingPayment = 5,
    ChequeDeposit = 6,
    ChequeClearance = 7,
    PayrollRun = 8,
    Depreciation = 9,
    InventoryShipment = 10,
    InventoryReceipt = 11,
    ExchangeVariance = 12
}

/// <summary>
/// Logical role an account plays in a posting profile (e.g. "Tax", "Revenue", "AR").
/// </summary>
public enum PostingAccountRole
{
    AccountsReceivable = 1,
    AccountsPayable = 2,
    Revenue = 3,
    CostOfGoodsSold = 4,
    InventoryAsset = 5,
    TaxPayable = 6,
    TaxReceivable = 7,
    DiscountExpense = 8,
    ExchangeGain = 9,
    ExchangeLoss = 10,
    BankCash = 11,
    ChequePortfolio = 12,
    ChequeInCollection = 13,
    SalaryExpense = 14,
    SocialSecurityExpense = 15,
    SocialSecurityPayable = 16
}

/// <summary>
/// Core tax classifications for TRNC and generic accounting.
/// </summary>
public enum TaxType
{
    KDV = 1,          // Value Added Tax
    Stopaj = 2,       // Withholding Tax
    Damga = 3,        // Stamp Duty
    OzelIletisim = 4, // Special Communications Tax
    Kurumlar = 5,     // Corporate Tax
    Gelir = 6         // Income Tax
}
