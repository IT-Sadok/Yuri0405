using Application.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Mediator;

public class MediatorTests
{
    private record PingRequest(string Message) : IRequest<string>;

    private class PingHandler : IRequestHandler<PingRequest, string>
    {
        public Task<string> Handle(PingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult($"pong: {request.Message}");
    }

    [Fact]
    public async Task Send_RegisteredHandler_ReturnsHandlerResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<PingRequest, string>, PingHandler>();
        var mediator = new Application.Mediator.Mediator(services.BuildServiceProvider());

        // Act
        var result = await mediator.Send(new PingRequest("hello"));

        // Assert
        Assert.Equal("pong: hello", result);
    }

    [Fact]
    public async Task Send_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mediator = new Application.Mediator.Mediator(new ServiceCollection().BuildServiceProvider());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new PingRequest("hello")));
    }
}
