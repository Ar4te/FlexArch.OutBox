using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.Options;
using Polly;

namespace FlexArch.OutBox.Core.Middlewares;

public class RetryMiddleware : IOutboxMiddleware
{
    private readonly int _maxRetryCount;
    private readonly TimeSpan _retryDelay;

    public RetryMiddleware(IOptions<RetryOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _maxRetryCount = options.Value.MaxRetryCount;
        _retryDelay = TimeSpan.FromSeconds(options.Value.DelayInSeconds);
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(next);

        Polly.Retry.AsyncRetryPolicy policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(_maxRetryCount, _ => _retryDelay);

        await policy.ExecuteAsync(() => next(message));
    }
}
