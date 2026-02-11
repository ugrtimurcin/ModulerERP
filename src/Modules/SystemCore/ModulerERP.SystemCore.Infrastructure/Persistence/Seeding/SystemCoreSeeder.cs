using ModulerERP.SystemCore.Application.Constants;
using ModulerERP.SystemCore.Domain.Entities;

namespace ModulerERP.SystemCore.Infrastructure.Persistence.Seeding;

/// <summary>
/// Initial seed data for the system.
/// </summary>
public static class SystemCoreSeeder
{
    // Well-known IDs for seed data
    // Well-known IDs for seed data
    public static readonly Guid RootTenantId = SystemCoreConstants.SystemTenantId;
    public static readonly Guid AdminUserId = SystemCoreConstants.SystemAdminUserId;
    public static readonly Guid AdminRoleId = SystemCoreConstants.SystemAdminRoleId;
    
    // Currency IDs
    public static readonly Guid TryId = new("00000000-0000-0000-0001-000000000001");
    public static readonly Guid UsdId = new("00000000-0000-0000-0001-000000000002");
    public static readonly Guid EurId = new("00000000-0000-0000-0001-000000000003");
    public static readonly Guid GbpId = new("00000000-0000-0000-0001-000000000004");
    
    // Language IDs
    public static readonly Guid TurkishId = new("00000000-0000-0000-0002-000000000001");
    public static readonly Guid EnglishId = new("00000000-0000-0000-0002-000000000002");

    public static IEnumerable<Currency> GetCurrencies()
    {
        return new[]
        {
            CreateCurrency(TryId, "TRY", "Turkish Lira", "₺"),
            CreateCurrency(UsdId, "USD", "US Dollar", "$"),
            CreateCurrency(EurId, "EUR", "Euro", "€"),
            CreateCurrency(GbpId, "GBP", "British Pound", "£")
        };
    }

    public static IEnumerable<Language> GetLanguages()
    {
        return new[]
        {
            CreateLanguage(TurkishId, "tr", "Türkçe"),
            CreateLanguage(EnglishId, "en", "English")
        };
    }

    public static Tenant GetRootTenant()
    {
        var tenant = Tenant.Create(
            "Root",
            "TRNC-ERP",
            TryId,
            "Europe/Nicosia");
        
        // Use reflection to set the Id for seeding
        SetEntityId(tenant, RootTenantId);
        return tenant;
    }

    public static Role GetAdminRole()
    {
        var role = Role.Create(
            RootTenantId,
            "Administrator",
            "Full system access",
            isSystemRole: true);
        
        SetEntityId(role, AdminRoleId);
        return role;
    }

    public static User GetAdminUser()
    {
        // Generate proper BCrypt hash for "Admin123!"
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        
        var user = User.Create(
            RootTenantId,
            "admin@erp.local",
            passwordHash,
            "System",
            "Administrator",
            AdminUserId);
        
        SetEntityId(user, AdminUserId);
        return user;
    }

    private static Currency CreateCurrency(Guid id, string code, string name, string symbol)
    {
        var currency = Currency.Create(code, name, symbol, 2);
        SetEntityId(currency, id);
        return currency;
    }

    private static Language CreateLanguage(Guid id, string code, string name)
    {
        var language = Language.Create(code, name);
        SetEntityId(language, id);
        return language;
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        // Try property first
        var idProperty = typeof(T).GetProperty("Id") 
            ?? typeof(T).BaseType?.GetProperty("Id");
        
        if (idProperty != null)
        {
            // Try backing field for auto-property
            var field = typeof(T).GetField("<Id>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (field == null)
            {
                // Try BaseEntity's field
                var baseType = typeof(T).BaseType;
                while (baseType != null && field == null)
                {
                    field = baseType.GetField("<Id>k__BackingField",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    baseType = baseType.BaseType;
                }
            }
            
            field?.SetValue(entity, id);
        }
    }

    private static void SetUserPassword(User user, string passwordHash)
    {
        var field = typeof(User).GetField("<PasswordHash>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(user, passwordHash);
    }
}
