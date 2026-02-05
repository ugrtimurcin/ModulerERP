namespace ModulerERP.CRM.Domain.Enums;

/// <summary>
/// Partner kind: Company or Individual
/// </summary>
public enum PartnerKind
{
    Company = 1,
    Individual = 2
}

/// <summary>
/// Lead status in the sales funnel
/// </summary>
public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Junk = 3
}

/// <summary>
/// Opportunity stage in the sales pipeline
/// </summary>
public enum OpportunityStage
{
    Discovery = 0,
    Proposal = 1,
    Negotiation = 2,
    Won = 3,
    Lost = 4
}

/// <summary>
/// Activity types for CRM interactions
/// </summary>
public enum ActivityType
{
    Call = 0,
    Email = 1,
    Meeting = 2,
    Note = 3
}

/// <summary>
/// Support ticket priority
/// </summary>
public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Support ticket status
/// </summary>
public enum TicketStatus
{
    Open = 0,
    Pending = 1,
    Resolved = 2,
    Closed = 3
}

/// <summary>
/// Commission calculation basis
/// </summary>
public enum CommissionBasis
{
    InvoicedAmount = 1,
    CollectedAmount = 2,
    GrossProfit = 3
}
