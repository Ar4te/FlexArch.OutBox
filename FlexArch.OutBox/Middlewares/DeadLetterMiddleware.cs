using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Abstractions.IStores;
using Microsoft.Extensions.Logging;

namespace FlexArch.OutBox.Core.Middlewares;

public class DeadLetterMiddleware : IOutboxMiddleware
{
    private readonly IDeadLetterStore _store;
    private readonly ILogger<DeadLetterMiddleware> _logger;

    public DeadLetterMiddleware(IDeadLetterStore store, ILogger<DeadLetterMiddleware> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        try
        {
            await next(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Outbox] DeadLetter - Message {Id} failed. Moving to DLQ", message.Id);
            await _store.SaveAsync(message, ex.Message);
        }
    }
}
