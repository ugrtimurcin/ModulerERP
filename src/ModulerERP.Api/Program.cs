using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Infrastructure;
using ModulerERP.SystemCore.Infrastructure.Persistence;
using ModulerERP.SystemCore.Infrastructure.Persistence.Seeding;
using ModulerERP.SystemCore.Application;
using ModulerERP.CRM.Infrastructure;
using ModulerERP.Inventory.Infrastructure;
using ModulerERP.Sales.Infrastructure;
using ModulerERP.Finance.Infrastructure;
using ModulerERP.HR.Infrastructure;
using ModulerERP.FixedAssets.Infrastructure;
using ModulerERP.FixedAssets.Application;
using ModulerERP.Manufacturing.Infrastructure;
using ModulerERP.Procurement.Infrastructure;
using ModulerERP.ProjectManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<ModulerERP.SharedKernel.Interfaces.ICurrentUserService, ModulerERP.Api.Services.CurrentUserService>();
builder.Services.AddScoped<ModulerERP.ProjectManagement.Application.Interfaces.IResourceCostProvider, ModulerERP.Api.Services.ResourceCostProvider>();



// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175") // Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SystemCore module services (DbContext, Auth, Services)
builder.Services.AddSystemCoreInfrastructure(builder.Configuration);
builder.Services.AddSystemCoreApplication();

// Add CRM module services
builder.Services.AddCRMInfrastructure(builder.Configuration);

// Add Inventory module services
// Add Inventory module services
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// Add Sales module services
builder.Services.AddSalesInfrastructure(builder.Configuration);

// Add Finance module services
// Add Finance module services
builder.Services.AddFinanceInfrastructure(builder.Configuration);

// Add HR module services
builder.Services.AddHRInfrastructure(builder.Configuration);

// Add Fixed Assets module services
builder.Services.AddFixedAssetsInfrastructure(builder.Configuration);
builder.Services.AddFixedAssetsApplication();

// Add Manufacturing module services
builder.Services.AddManufacturingModule(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add Procurement module services

// Add Procurement module services
builder.Services.AddProcurementInfrastructure(builder.Configuration);

// Add Project Management module services
builder.Services.AddProjectManagementInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("DevCors");
    
    // Auto-apply migrations and seed in development
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SystemCoreDbContext>();
        
        // Apply migrations
        await context.Database.MigrateAsync();
        
        // Check for --seed argument
        if (args.Contains("--seed"))
        {
            // Seed Currencies if missing
            if (!await context.Currencies.AnyAsync())
            {
                context.Currencies.AddRange(SystemCoreSeeder.GetCurrencies());
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded Currencies.");
            }

            // Seed Languages if missing
            if (!await context.Languages.AnyAsync())
            {
                context.Languages.AddRange(SystemCoreSeeder.GetLanguages());
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded Languages.");
            }

            var rootTenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == SystemCoreSeeder.RootTenantId);
            if (rootTenant == null)
            {
                context.Tenants.Add(SystemCoreSeeder.GetRootTenant());
                context.Roles.Add(SystemCoreSeeder.GetAdminRole());
                context.Users.Add(SystemCoreSeeder.GetAdminUser());
                
                // Assign role
                context.UserRoles.Add(ModulerERP.SystemCore.Domain.Entities.UserRole.Create(
                    SystemCoreSeeder.RootTenantId, 
                    SystemCoreSeeder.AdminUserId, 
                    SystemCoreSeeder.AdminRoleId));

                try
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine("Seeding completed: Root Tenant and Admin User created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Seeding failed: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
            else
            {
                 Console.WriteLine("Seeding skipped: Root Tenant already exists.");
            }
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
