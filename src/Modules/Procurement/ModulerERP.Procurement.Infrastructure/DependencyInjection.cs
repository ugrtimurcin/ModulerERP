using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.Procurement.Application.Interfaces;
using ModulerERP.Procurement.Infrastructure.Persistence;
using ModulerERP.Procurement.Infrastructure.Services;

namespace ModulerERP.Procurement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProcurementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProcurementDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_Procurement", "procurement")));

        services.AddScoped<IRfqService, RfqService>();
        services.AddScoped<IPurchaseQuoteService, PurchaseQuoteService>();
        services.AddScoped<IQcService, QcService>();
        services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();

        return services;
    }
}
