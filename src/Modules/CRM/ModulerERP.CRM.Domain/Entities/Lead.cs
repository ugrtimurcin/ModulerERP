using ModulerERP.SharedKernel.Entities;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Pre-qualified potential customer.
/// Optional - can create partners directly in TRNC market.
/// </summary>
public class Lead : BaseEntity
{
    /// <summary>e.g., 'Mr.', 'Mrs.', 'Dr.'</summary>
    public string? Title { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Company { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    
    public LeadStatus Status { get; private set; } = LeadStatus.New;
    
    /// <summary>Origin (e.g., 'Website', 'Trade Show', 'Referral')</summary>
    public string? Source { get; private set; }
    
    /// <summary>Priority sales rep</summary>
    public Guid? AssignedUserId { get; private set; }
    
    /// <summary>Converted to BusinessPartner</summary>
    public Guid? ConvertedPartnerId { get; private set; }
    
    public DateTime? ConvertedAt { get; private set; }

    public Guid? TerritoryId { get; private set; }
    public Guid? RejectionReasonId { get; private set; }

    // KVKK / GDPR Compliance
    public bool IsMarketingConsentGiven { get; private set; }
    public DateTime? ConsentDate { get; private set; }
    public string? ConsentSource { get; private set; }

    // Navigation
    public Territory? Territory { get; private set; }
    public RejectionReason? RejectionReason { get; private set; }
    public BusinessPartner? ConvertedPartner { get; private set; }
    public ICollection<Activity> Activities { get; private set; } = new List<Activity>();

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Lead() { } // EF Core

    public static Lead Create(
        Guid tenantId,
        string firstName,
        string lastName,
        Guid createdByUserId,
        string? title = null,
        string? company = null,
        string? email = null,
        string? phone = null,
        string? source = null,
        Guid? assignedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        var lead = new Lead
        {
            Title = title,
            FirstName = firstName,
            LastName = lastName,
            Company = company,
            Email = email,
            Phone = phone,
            Source = source,
            AssignedUserId = assignedUserId
        };

        lead.SetTenant(tenantId);
        lead.SetCreator(createdByUserId);
        return lead;
    }

    public void SetMarketingConsent(bool isGiven, string? source = null)
    {
        IsMarketingConsentGiven = isGiven;
        ConsentSource = source;
        if (isGiven) ConsentDate = DateTime.UtcNow;
        else ConsentDate = null;
    }

    public void SetTerritory(Guid? territoryId) => TerritoryId = territoryId;

    public void UpdateStatus(LeadStatus status) => Status = status;
    
    public void Assign(Guid userId) => AssignedUserId = userId;

    public void ConvertToPartner(Guid partnerId)
    {
        ConvertedPartnerId = partnerId;
        ConvertedAt = DateTime.UtcNow;
        Status = LeadStatus.Qualified;
    }

    public void MarkAsJunk(Guid? rejectionReasonId)
    {
        Status = LeadStatus.Junk;
        RejectionReasonId = rejectionReasonId;
    }
}
