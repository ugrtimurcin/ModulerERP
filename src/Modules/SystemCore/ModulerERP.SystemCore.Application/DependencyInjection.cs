using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace ModulerERP.SystemCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSystemCoreApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
