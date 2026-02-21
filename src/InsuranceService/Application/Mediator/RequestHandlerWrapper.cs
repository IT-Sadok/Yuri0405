namespace Application.Mediator;

internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct);
}

internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct)
    {
        var handler = (IRequestHandler<TRequest, TResponse>?)sp.GetService(typeof(IRequestHandler<TRequest, TResponse>))
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");
        return handler.Handle((TRequest)request, ct);
    }
}
