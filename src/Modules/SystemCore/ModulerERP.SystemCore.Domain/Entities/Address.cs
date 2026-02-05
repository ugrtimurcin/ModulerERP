using ModulerERP.SharedKernel.Entities;
using ModulerERP.SystemCore.Domain.Enums;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Shared address resource for Users, Warehouses, Leads, Partners.
/// Decoupled from CRM for reusability across modules.
/// </summary>
public class Address : BaseEntity
{
    /// <summary>Friendly name (e.g., 'Kyrenia Branch', 'Head Office')</summary>
    public string Name { get; private set; } = string.Empty;
    
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    
    /// <summary>Entity type: Partner, Warehouse, User, etc.</summary>
    public AddressEntityType EntityType { get; private set; }
    
    /// <summary>ID of the linked entity</summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>Is this the primary address for the entity?</summary>
    public bool IsDefault { get; private set; }

    private Address() { } // EF Core

    public static Address Create(
        Guid tenantId,
        string name,
        AddressEntityType entityType,
        Guid entityId,
        Guid createdByUserId,
        string? street = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? postalCode = null,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Address name is required", nameof(name));

        var address = new Address
        {
            Name = name,
            Street = street,
            City = city,
            State = state,
            Country = country,
            PostalCode = postalCode,
            EntityType = entityType,
            EntityId = entityId,
            IsDefault = isDefault
        };

        address.SetTenant(tenantId);
        address.SetCreator(createdByUserId);
        return address;
    }

    public void Update(string name, string? street, string? city, string? state, string? country, string? postalCode)
    {
        Name = name;
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public void SetAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;

    public string FullAddress => string.Join(", ", 
        new[] { Street, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
