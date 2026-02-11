using Microsoft.Extensions.DependencyInjection;
using ModulerERP.FixedAssets.Application.Interfaces;
using ModulerERP.FixedAssets.Application.Services;

namespace ModulerERP.FixedAssets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFixedAssetsApplication(this IServiceCollection services)
    {
        services.AddScoped<IFixedAssetService, FixedAssetService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
