using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace FlexArch.OutBox.Core.Middlewares;

public class CircuitBreakerMiddleware : IOutboxMiddleware
{
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly ILogger<CircuitBreakerMiddleware> _logger;

    public CircuitBreakerMiddleware(ILogger<CircuitBreakerMiddleware> logger, IOptions<CircuitBreakerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(options);

        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                options.Value.FailureThreshold, // 失败阈值
                TimeSpan.FromSeconds(options.Value.DurationOfBreakInSeconds), // 熔断持续时间
                onBreak: (exception, timeSpan) =>
                {
                    // 记录熔断触发时的日志
                    _logger.LogCritical(exception, "Circuit breaker triggered: {Message}", exception.Message);
                },
                onReset: () =>
                {
                    // 记录熔断重置时的日志
                    _logger.LogCritical("Circuit breaker reset.");
                }
            );
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            // 使用熔断策略来控制消息发布
            await _circuitBreakerPolicy.ExecuteAsync(() => next(message));
        }
        catch (BrokenCircuitException)
        {
            // 熔断器触发时的处理
            Console.WriteLine("Circuit breaker is open, skipping message publish.");
        }
    }
}
