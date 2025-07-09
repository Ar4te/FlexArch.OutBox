using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions;

public class MiddlewareOutboxPublisher : IOutboxPublisher
{
    private readonly IList<IOutboxMiddleware> _middlewares;
    private readonly OutboxPublishDelegate _finalPublisher;

    public MiddlewareOutboxPublisher(IEnumerable<IOutboxMiddleware> middlewares, OutboxPublishDelegate finalPublisher)
    {
        _middlewares = middlewares.ToList();
        _finalPublisher = finalPublisher;
    }

    public Task PublishAsync(IOutboxMessage outboxMessage)
    {
        OutboxPublishDelegate pipeline = _finalPublisher;
        foreach (IOutboxMiddleware? middleware in _middlewares.Reverse())
        {
            OutboxPublishDelegate next = pipeline;
            pipeline = (msg) => middleware.InvokeAsync(msg, next);
        }

        return pipeline(outboxMessage);
    }
}
