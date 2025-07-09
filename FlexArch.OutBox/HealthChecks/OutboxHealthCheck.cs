using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlexArch.OutBox.Core.HealthChecks;

/// <summary>
/// OutBox健康检查，监控消息处理状态和系统健康
/// </summary>
public class OutboxHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxHealthCheck> _logger;
    private readonly OutboxOptions _options;

    public OutboxHealthCheck(
        IServiceProvider serviceProvider,
        ILogger<OutboxHealthCheck> logger,
        IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IOutboxStore store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

            // 检查未处理消息数量
            IReadOnlyList<Abstractions.IModels.IOutboxMessage> unsentMessages = await store.FetchUnsentMessagesAsync(1000); // 检查更大的批次以获得准确计数
            int unsentCount = unsentMessages.Count;

            var data = new Dictionary<string, object>
            {
                ["UnsentMessageCount"] = unsentCount,
                ["BatchSize"] = _options.BatchSize,
                ["ProcessingInterval"] = _options.ProcessingInterval.TotalSeconds
            };

            // 根据未处理消息数量判断健康状态
            if (unsentCount == 0)
            {
                return HealthCheckResult.Healthy("No unsent messages in outbox", data);
            }

            if (unsentCount < _options.BatchSize * 2)
            {
                return HealthCheckResult.Healthy($"Outbox has {unsentCount} unsent messages (within acceptable range)", data);
            }

            if (unsentCount < _options.BatchSize * 5)
            {
                return HealthCheckResult.Degraded($"Outbox has {unsentCount} unsent messages (moderate backlog)", data: data);
            }

            return HealthCheckResult.Unhealthy($"Outbox has {unsentCount} unsent messages (severe backlog)", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutBox health check failed");
            return HealthCheckResult.Unhealthy("Failed to check outbox health", ex);
        }
    }
}
