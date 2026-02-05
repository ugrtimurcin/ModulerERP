using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Application.Services;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Infrastructure.Persistence;
using ModulerERP.Inventory.Infrastructure.Services;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        
        // Ensure InventoryDbContext implements IUnitOfWork or wrap it?
        // Wait, InventoryDbContext does NOT implement IUnitOfWork in definition Step 3535.
        // It inherits DbContext. IRepository interface definition Step 3564 defines IUnitOfWork.
        // I need to make InventoryDbContext implement IUnitOfWork or create an adapter.
        // Let's modify InventoryDbContext to implement IUnitOfWork since it matches SaveChangesAsync signature.
        
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
        services.AddScoped<IProductService, ProductService>();
        
        // Register Repositories
        services.AddScoped<IRepository<StockMovement>, InventoryRepository<StockMovement>>();
        services.AddScoped<IRepository<StockLevel>, InventoryRepository<StockLevel>>();
        services.AddScoped<IRepository<Product>, InventoryRepository<Product>>();
        services.AddScoped<IRepository<Warehouse>, InventoryRepository<Warehouse>>();
        
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockReservationService, StockReservationService>();
        services.AddScoped<IStockOperationsService, StockOperationsService>();
        
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<IRepository<ProductVariant>, InventoryRepository<ProductVariant>>();
        
        // Extended Inventory Services
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IUnitConversionService, UnitConversionService>();
        services.AddScoped<IProductSerialService, ProductSerialService>();
        services.AddScoped<IProductBatchService, ProductBatchService>();
        services.AddScoped<IAttributeService, AttributeService>();

        return services;
    }
}
