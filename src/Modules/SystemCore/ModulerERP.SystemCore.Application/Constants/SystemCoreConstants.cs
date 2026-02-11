namespace ModulerERP.SystemCore.Application.Constants;

public static class SystemCoreConstants
{
    // Well-known IDs for the System/Host Tenant
    public static readonly Guid SystemTenantId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid SystemAdminRoleId = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid SystemAdminUserId = new("00000000-0000-0000-0000-000000000001");
    // Standard Currency IDs
    public static readonly Guid UsdCurrencyId = new("00000000-0000-0000-0000-000000000010");
}
