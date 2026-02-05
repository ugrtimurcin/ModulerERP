using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.Sales.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Application.Services;
using ModulerERP.Sales.Infrastructure.Services;


namespace ModulerERP.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShipmentService, ShipmentService>();

        services.AddScoped<ISalesOperationsService, SalesOperationsService>();
        
        return services;
    }
}
