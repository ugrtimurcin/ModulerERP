using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.Manufacturing.Application.Interfaces;
using ModulerERP.Manufacturing.Infrastructure.Persistence;
using ModulerERP.Manufacturing.Infrastructure.Services;

namespace ModulerERP.Manufacturing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddManufacturingModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ManufacturingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBomService, BomService>();
        services.AddScoped<IProductionOrderService, ProductionOrderService>();

        return services;
    }
}
