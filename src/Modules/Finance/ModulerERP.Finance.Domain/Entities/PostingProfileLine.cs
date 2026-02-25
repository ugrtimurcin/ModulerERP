using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// Defines a specific account binding for a role within a posting profile.
/// </summary>
public class PostingProfileLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public Guid PostingProfileId { get; private set; }
    
    public PostingAccountRole Role { get; private set; }
    
    /// <summary>Specific GL Account to post debits/credits to for this role</summary>
    public Guid AccountId { get; private set; }

    // Navigation
    public PostingProfile? PostingProfile { get; private set; }
    public Account? Account { get; private set; }

    private PostingProfileLine() { } // EF Core

    internal static PostingProfileLine Create(Guid postingProfileId, PostingAccountRole role, Guid accountId)
    {
        return new PostingProfileLine
        {
            PostingProfileId = postingProfileId,
            Role = role,
            AccountId = accountId
        };
    }
}
