using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace MediatR;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> _requestHandlers = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        
        var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(requestType,
            t => Activator.CreateInstance(typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return handler.Handle(request, cancellationToken, _serviceProvider);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        return Send((IRequest<Unit>)request, cancellationToken);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Object send not implemented in custom MediatR");
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handler.GetType().GetMethod("Handle");
            if (method != null)
            {
                await (Task)method.Invoke(handler, new[] { notification, cancellationToken })!;
            }
        }
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Publish((object)notification, cancellationToken);
    }
}

internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider);
}

internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(object request, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
        }

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();

        RequestHandlerDelegate<TResponse> next = () => handler.Handle((TRequest)request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var nextBehavior = next;
            next = () => behavior.Handle((TRequest)request, nextBehavior, cancellationToken);
        }

        return next();
    }
}
