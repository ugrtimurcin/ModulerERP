using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// User entity for authentication identity.
/// Focuses strictly on Identity - roles managed via UserRoles N-N relationship.
/// </summary>
public class User : BaseEntity
{
    /// <summary>Unique Login ID per Tenant</summary>
    public string Email { get; private set; } = string.Empty;
    
    /// <summary>Bcrypt/Argon2 hash</summary>
    public string PasswordHash { get; private set; } = string.Empty;
    
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    
    /// <summary>Security policy flag for 2FA</summary>
    public bool TwoFactorEnabled { get; private set; }
    
    /// <summary>Brute-force protection counter</summary>
    public int FailedLoginAttempts { get; private set; }
    
    /// <summary>Account suspension end time</summary>
    public DateTime? LockoutEnd { get; private set; }
    
    /// <summary>Last successful login timestamp</summary>
    public DateTime? LastLoginDate { get; private set; }
    
    /// <summary>For mandatory rotation policies</summary>
    public DateTime? LastPasswordChangeDate { get; private set; }

    // Navigation
    public ICollection<UserSession> Sessions { get; private set; } = new List<UserSession>();
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public string FullName => $"{FirstName} {LastName}".Trim();

    private User() { } // EF Core

    public static User Create(Guid tenantId, string email, string passwordHash, string firstName, string lastName, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        var user = new User
        {
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            LastPasswordChangeDate = DateTime.UtcNow
        };
        
        user.SetTenant(tenantId);
        user.SetCreator(createdByUserId);
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangeDate = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = DateTime.UtcNow;
    }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public void EnableTwoFactor() => TwoFactorEnabled = true;
    public void DisableTwoFactor() => TwoFactorEnabled = false;
}
