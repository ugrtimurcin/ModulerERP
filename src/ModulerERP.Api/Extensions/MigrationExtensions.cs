using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.Finance.Infrastructure.Persistence;
using ModulerERP.FixedAssets.Infrastructure.Persistence;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.Inventory.Infrastructure.Persistence;
using ModulerERP.Manufacturing.Infrastructure.Persistence;
using ModulerERP.Procurement.Infrastructure.Persistence;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyModuleMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        // 1. System Core (Must be first - Tenants, Users, Roles)
        var systemContext = services.GetRequiredService<SystemCoreDbContext>();
        await systemContext.Database.MigrateAsync();
        
        // 2. Modules (Order generally doesn't matter if schemas are isolated, but good to be deterministic)
        var hrContext = services.GetRequiredService<HRDbContext>();
        await hrContext.Database.MigrateAsync();

        var fixedAssetsContext = services.GetRequiredService<FixedAssetsDbContext>();
        await fixedAssetsContext.Database.MigrateAsync();

        var inventoryContext = services.GetRequiredService<InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var procurementContext = services.GetRequiredService<ProcurementDbContext>();
        await procurementContext.Database.MigrateAsync();

        var manufacturingContext = services.GetRequiredService<ManufacturingDbContext>();
        await manufacturingContext.Database.MigrateAsync();

        var salesContext = services.GetRequiredService<SalesDbContext>();
        await salesContext.Database.MigrateAsync();

        var crmContext = services.GetRequiredService<CRMDbContext>();
        await crmContext.Database.MigrateAsync();

        var financeContext = services.GetRequiredService<FinanceDbContext>();
        await financeContext.Database.MigrateAsync();

        var projectContext = services.GetRequiredService<ProjectManagementDbContext>();
        await projectContext.Database.MigrateAsync();
        
        Console.WriteLine("All module migrations applied successfully.");
    }
}
