using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Generic posting configuration mapping business events to GL accounts.
/// </summary>
public class PostingProfile : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>e.g., SalesInvoice, PurchaseInvoice, Payment</summary>
    public TransactionType TransactionType { get; private set; }
    
    /// <summary>e.g., 'Standard Goods', 'Services'</summary>
    public string? Category { get; private set; }

    /// <summary>Is this the default profile for this transaction type/category?</summary>
    public bool IsDefault { get; private set; }

    public ICollection<PostingProfileLine> Lines { get; private set; } = new List<PostingProfileLine>();

    private PostingProfile() { } // EF Core

    public static PostingProfile Create(
        Guid tenantId,
        string code,
        string name,
        TransactionType transactionType,
        Guid createdByUserId,
        string? category = null,
        bool isDefault = false)
    {
        var profile = new PostingProfile
        {
            Code = code,
            Name = name,
            TransactionType = transactionType,
            Category = category,
            IsDefault = isDefault
        };

        profile.SetTenant(tenantId);
        profile.SetCreator(createdByUserId);
        return profile;
    }

    public void AddLine(PostingAccountRole role, Guid accountId)
    {
        // Simple distinct role validation can go here if a role should only appear once per profile type
        Lines.Add(PostingProfileLine.Create(Id, role, accountId));
    }

    public void Update(string name, string? category, bool isDefault)
    {
        Name = name;
        Category = category;
        IsDefault = isDefault;
    }
}
