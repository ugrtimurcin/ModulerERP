using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.FixedAssets.Application;
using ModulerERP.FixedAssets.Domain.Entities;
using ModulerERP.FixedAssets.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.FixedAssets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFixedAssetsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FixedAssetsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(FixedAssetsDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FixedAssetsDbContext>());

        // Register Repositories
        services.AddScoped<IRepository<Asset>, FixedAssetsRepository<Asset>>();
        services.AddScoped<IRepository<AssetCategory>, FixedAssetsRepository<AssetCategory>>();
        services.AddScoped<IRepository<AssetAssignment>, FixedAssetsRepository<AssetAssignment>>();
        services.AddScoped<IRepository<AssetMeterLog>, FixedAssetsRepository<AssetMeterLog>>();
        services.AddScoped<IRepository<AssetIncident>, FixedAssetsRepository<AssetIncident>>();
        services.AddScoped<IRepository<AssetMaintenance>, FixedAssetsRepository<AssetMaintenance>>();
        services.AddScoped<IRepository<AssetDisposal>, FixedAssetsRepository<AssetDisposal>>();

        // Add Application Services
        services.AddFixedAssetsApplication();

        return services;
    }
}
