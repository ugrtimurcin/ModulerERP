using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Application.Services;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddScoped<ILedgerPostingService, LedgerPostingService>();
        services.AddScoped<IFinanceOperationsService, FinanceOperationsService>();
        services.AddScoped<IFiscalPeriodClosingService, FiscalPeriodClosingService>();

        return services;
    }
}
