using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.ValueObjects;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Sales pipeline with weighted forecasting.
/// Uses Money value object for type-safe currency handling.
/// </summary>
public class Opportunity : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    
    public Guid? LeadId { get; private set; }
    public Guid? PartnerId { get; private set; }
    
    /// <summary>Estimated deal value with currency (type-safe)</summary>
    public Money EstimatedValue { get; private set; } = Money.Zero("TRY");
    
    /// <summary>FK to Currencies table for relational integrity</summary>
    public Guid? CurrencyId { get; private set; }
    
    public OpportunityStage Stage { get; private set; } = OpportunityStage.Discovery;
    
    /// <summary>Percentage for forecasting (0-100)</summary>
    public int Probability { get; private set; }
    
    public DateTime? ExpectedCloseDate { get; private set; }
    
    /// <summary>Responsible sales rep</summary>
    public Guid? AssignedUserId { get; private set; }

    // Navigation
    public Lead? Lead { get; private set; }
    public BusinessPartner? Partner { get; private set; }
    public ICollection<Activity> Activities { get; private set; } = new List<Activity>();

    /// <summary>Weighted value for forecasting</summary>
    public decimal WeightedValue => EstimatedValue.Amount * (Probability / 100m);

    private Opportunity() { } // EF Core

    public static Opportunity Create(
        Guid tenantId,
        string title,
        decimal estimatedValue,
        string currencyCode,
        Guid createdByUserId,
        Guid? leadId = null,
        Guid? partnerId = null,
        Guid? currencyId = null,
        Guid? assignedUserId = null,
        DateTime? expectedCloseDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        var opportunity = new Opportunity
        {
            Title = title,
            LeadId = leadId,
            PartnerId = partnerId,
            EstimatedValue = Money.Create(estimatedValue, currencyCode),
            CurrencyId = currencyId,
            AssignedUserId = assignedUserId,
            ExpectedCloseDate = expectedCloseDate,
            Probability = 10 // Discovery default
        };

        opportunity.SetTenant(tenantId);
        opportunity.SetCreator(createdByUserId);
        return opportunity;
    }

    public void UpdateStage(OpportunityStage stage)
    {
        Stage = stage;
        // Auto-adjust probability based on stage
        Probability = stage switch
        {
            OpportunityStage.Discovery => 10,
            OpportunityStage.Proposal => 30,
            OpportunityStage.Negotiation => 60,
            OpportunityStage.Won => 100,
            OpportunityStage.Lost => 0,
            _ => Probability
        };
    }

    public void UpdateValue(decimal estimatedValue, string currencyCode, Guid? currencyId)
    {
        EstimatedValue = Money.Create(estimatedValue, currencyCode);
        CurrencyId = currencyId;
    }

    public void SetProbability(int probability)
    {
        if (probability < 0 || probability > 100)
            throw new ArgumentException("Probability must be between 0 and 100");
        Probability = probability;
    }

    public void Assign(Guid userId) => AssignedUserId = userId;
    
    public void MarkAsWon() => UpdateStage(OpportunityStage.Won);
    public void MarkAsLost() => UpdateStage(OpportunityStage.Lost);
}
