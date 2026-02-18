using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.SharedKernel.Behaviors;

namespace ModulerERP.CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
