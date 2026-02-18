using ModulerERP.SharedKernel.Entities;
using ModulerERP.SharedKernel.ValueObjects;
using ModulerERP.CRM.Domain.Enums;

namespace ModulerERP.CRM.Domain.Entities;

/// <summary>
/// Central entity for Customers and Suppliers.
/// TRNC-specific features: multi-currency, granular phone fields, TRNC-aware addresses.
/// </summary>
public class BusinessPartner : BaseEntity
{
    /// <summary>Unique ERP Code (e.g., 'C-001')</summary>
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>Commercial Title / Full Name</summary>
    public string Name { get; private set; } = string.Empty;
    
    public bool IsCustomer { get; private set; }
    public bool IsSupplier { get; private set; }
    
    /// <summary>Official Tax Office</summary>
    public string? TaxOffice { get; private set; }
    
    /// <summary>VKN (For Companies)</summary>
    public string? TaxNumber { get; private set; }
    
    /// <summary>TCKN (For Individuals)</summary>
    public string? IdentityNumber { get; private set; }
    
    public PartnerKind Kind { get; private set; } = PartnerKind.Company;
    
    public Guid? GroupId { get; private set; }
    
    /// <summary>For Holding/Branch hierarchy</summary>
    public Guid? ParentPartnerId { get; private set; }
    
    /// <summary>Default trading currency (Critical for TRNC contracts)</summary>
    public Guid? DefaultCurrencyId { get; private set; }
    
    /// <summary>Net days for due date calculation</summary>
    public int PaymentTermDays { get; private set; } = 30;
    
    /// <summary>Max allowed debt in DefaultCurrency</summary>
    public decimal CreditLimit { get; private set; }
    
    /// <summary>Base discount % applied to invoice totals</summary>
    public decimal DefaultDiscountRate { get; private set; }
    
    public string? Website { get; private set; }
    public string? Email { get; private set; }
    public string? MobilePhone { get; private set; }
    public string? Landline { get; private set; }
    public string? Fax { get; private set; }
    public string? WhatsappNumber { get; private set; }

    // ── TRNC-Aware Addresses ──
    /// <summary>Official billing/registration address</summary>
    public Address? BillingAddress { get; private set; }

    /// <summary>Goods delivery address (may differ from billing)</summary>
    public Address? ShippingAddress { get; private set; }

    // Navigation
    public BusinessPartnerGroup? Group { get; private set; }
    public BusinessPartner? ParentPartner { get; private set; }
    public ICollection<BusinessPartner> ChildPartners { get; private set; } = new List<BusinessPartner>();
    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();
    public ICollection<PartnerBalance> Balances { get; private set; } = new List<PartnerBalance>();

    private BusinessPartner() { } // EF Core

    public static BusinessPartner Create(
        Guid tenantId,
        string code,
        string name,
        PartnerKind kind,
        bool isCustomer,
        bool isSupplier,
        Guid createdByUserId,
        Guid? groupId = null,
        Guid? defaultCurrencyId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Partner code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Partner name is required", nameof(name));
        if (!isCustomer && !isSupplier)
            throw new ArgumentException("Partner must be either customer or supplier");

        var partner = new BusinessPartner
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Kind = kind,
            IsCustomer = isCustomer,
            IsSupplier = isSupplier,
            GroupId = groupId,
            DefaultCurrencyId = defaultCurrencyId
        };

        partner.SetTenant(tenantId);
        partner.SetCreator(createdByUserId);
        return partner;
    }

    public void UpdateBasicInfo(string name, PartnerKind kind, bool isCustomer, bool isSupplier)
    {
        Name = name;
        Kind = kind;
        IsCustomer = isCustomer;
        IsSupplier = isSupplier;
    }

    public void UpdateTaxInfo(string? taxOffice, string? taxNumber, string? identityNumber)
    {
        TaxOffice = taxOffice;
        TaxNumber = taxNumber;
        IdentityNumber = identityNumber;
    }

    public void UpdateFinancialInfo(Guid? defaultCurrencyId, int paymentTermDays, decimal creditLimit, decimal defaultDiscountRate)
    {
        DefaultCurrencyId = defaultCurrencyId;
        PaymentTermDays = paymentTermDays;
        CreditLimit = creditLimit;
        DefaultDiscountRate = defaultDiscountRate;
    }

    public void UpdateContactInfo(string? website, string? email, string? mobilePhone, string? landline, string? fax, string? whatsappNumber)
    {
        Website = website;
        Email = email;
        MobilePhone = mobilePhone;
        Landline = landline;
        Fax = fax;
        WhatsappNumber = whatsappNumber;
    }

    public void UpdateBillingAddress(Address? address) => BillingAddress = address;
    public void UpdateShippingAddress(Address? address) => ShippingAddress = address;

    public void SetGroup(Guid? groupId) => GroupId = groupId;
    public void SetParentPartner(Guid? parentPartnerId) => ParentPartnerId = parentPartnerId;
}
