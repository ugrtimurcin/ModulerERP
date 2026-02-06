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

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174") // Vite default port
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
    // using (var scope = app.Services.CreateScope())
    // {
    //     var context = scope.ServiceProvider.GetRequiredService<SystemCoreDbContext>();
    //     var crmContext = scope.ServiceProvider.GetRequiredService<ModulerERP.CRM.Infrastructure.Persistence.CRMDbContext>();
        
    //     // NOTE: Set to true to reset database schema (delete and recreate)
    //     var resetDatabase = false;
    //     if (resetDatabase)
    //     {
    //         await context.Database.EnsureDeletedAsync();
    //     }
        
    //     // Apply migrations
    //     await context.Database.MigrateAsync();
    //     await crmContext.Database.MigrateAsync();
        
    //     // Seed initial data if empty
    //     // Seed initial data if empty
    //     /* 
    //      * Seeding logic removed to prevent conflicts with existing database.
    //      * The database is expected to be already populated or managed externally.
    //      */
    // }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
