using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.Sales.Infrastructure.Repositories;
using ModulerERP.Sales.Infrastructure.Services;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<ISalesUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(SalesRepository<>));

        // MediatR handlers from Application + Infrastructure assemblies
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ISalesUnitOfWork).Assembly);          // Application (Commands)
            cfg.RegisterServicesFromAssembly(typeof(SalesDbContext).Assembly);              // Infrastructure (Queries)
        });

        // Cross-module integration service
        services.AddScoped<ISalesOperationsService, SalesOperationsService>();

        return services;
    }
}
