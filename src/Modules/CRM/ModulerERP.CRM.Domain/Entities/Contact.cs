using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Individual people working at the Business Partner's company.
/// </summary>
public class Contact : BaseEntity
{
    public Guid PartnerId { get; private set; }
    
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    
    /// <summary>Job Title (e.g., 'Purchasing Manager')</summary>
    public string? Position { get; private set; }
    
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    
    /// <summary>Is this the main point of contact?</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Contact's office/personal address</summary>
    public Address? Address { get; private set; }

    // KVKK / GDPR Compliance
    public bool IsMarketingConsentGiven { get; private set; }
    public DateTime? ConsentDate { get; private set; }
    public string? ConsentSource { get; private set; }

    // Navigation
    public BusinessPartner? Partner { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Contact() { } // EF Core

    public static Contact Create(
        Guid tenantId,
        Guid partnerId,
        string firstName,
        string lastName,
        Guid createdByUserId,
        string? position = null,
        string? email = null,
        string? phone = null,
        bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        var contact = new Contact
        {
            PartnerId = partnerId,
            FirstName = firstName,
            LastName = lastName,
            Position = position,
            Email = email,
            Phone = phone,
            IsPrimary = isPrimary
        };

        contact.SetTenant(tenantId);
        contact.SetCreator(createdByUserId);
        return contact;
    }

    public void Update(string firstName, string lastName, string? position, string? email, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Position = position;
        Email = email;
        Phone = phone;
    }

    public void SetMarketingConsent(bool isGiven, string? source = null)
    {
        IsMarketingConsentGiven = isGiven;
        ConsentSource = source;
        if (isGiven) ConsentDate = DateTime.UtcNow;
        else ConsentDate = null;
    }

    public void UpdateAddress(Address? address) => Address = address;

    public void SetAsPrimary() => IsPrimary = true;
    public void RemovePrimary() => IsPrimary = false;
}
