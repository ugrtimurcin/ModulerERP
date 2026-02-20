using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ModulerERP.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}
