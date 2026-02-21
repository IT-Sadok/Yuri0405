namespace Application.Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var wrapperType = typeof(RequestHandlerWrapperImpl<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var wrapper = (RequestHandlerWrapper<TResponse>)Activator.CreateInstance(wrapperType)!;

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}
