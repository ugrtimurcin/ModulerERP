using System.Reflection;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatRServiceConfigurationExtensions
{
    public static IServiceCollection AddMediatR(this IServiceCollection services, Action<MediatRServiceConfiguration> configuration)
    {
        var config = new MediatRServiceConfiguration(services);
        configuration(config);
        
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        services.AddTransient<IPublisher, Mediator>();

        foreach (var assembly in config.AssembliesToRegister)
        {
            var types = assembly.GetTypes();

            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces();
                foreach (var i in interfaces)
                {
                    if (i.IsGenericType)
                    {
                        var genType = i.GetGenericTypeDefinition();
                        if (genType == typeof(IRequestHandler<,>) || 
                            genType == typeof(IRequestHandler<>) ||
                            genType == typeof(INotificationHandler<>))
                        {
                            services.AddTransient(i, type);
                            
                            // Map IRequestHandler<T> to IRequestHandler<T, Unit>
                            if (genType == typeof(IRequestHandler<>))
                            {
                                var requestType = i.GetGenericArguments()[0];
                                var fullType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(Unit));
                                var adapterType = typeof(RequestHandlerWrapperAdapter<>).MakeGenericType(requestType);
                                services.AddTransient(fullType, adapterType);
                            }
                        }
                    }
                }
            }
        }

        return services;
    }
}

public class RequestHandlerWrapperAdapter<TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest
{
    private readonly IRequestHandler<TRequest> _inner;
    public RequestHandlerWrapperAdapter(IRequestHandler<TRequest> inner)
    {
        _inner = inner;
    }
    public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await _inner.Handle(request, cancellationToken);
        return Unit.Value;
    }
}

public class MediatRServiceConfiguration
{
    private readonly IServiceCollection _services;
    public List<Assembly> AssembliesToRegister { get; } = new();

    public MediatRServiceConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public MediatRServiceConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        if (!AssembliesToRegister.Contains(assembly))
        {
            AssembliesToRegister.Add(assembly);
        }
        return this;
    }
    
    public MediatRServiceConfiguration AddOpenBehavior(Type openBehaviorType)
    {
        _services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
        return this;
    }
}
