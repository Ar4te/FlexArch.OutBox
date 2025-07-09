using FlexArch.OutBox.Abstractions.IStores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FlexArch.OutBox.Core.HealthChecks;

/// <summary>
/// 死信队列健康检查
/// </summary>
public class DeadLetterHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadLetterHealthCheck> _logger;

    public DeadLetterHealthCheck(
        IServiceProvider serviceProvider,
        ILogger<DeadLetterHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            IDeadLetterStore? deadLetterStore = scope.ServiceProvider.GetService<IDeadLetterStore>();
            if (deadLetterStore == null)
            {
                return HealthCheckResult.Healthy("Dead letter queue not configured");
            }

            int count = await deadLetterStore.CountAsync();

            var data = new Dictionary<string, object>
            {
                ["DeadLetterStoreConfigured"] = true,
                ["DeadLetterCount"] = count
            };

            return HealthCheckResult.Healthy("Dead letter queue is configured and accessible", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dead letter health check failed");
            return HealthCheckResult.Unhealthy("Failed to check dead letter queue health", ex);
        }
    }
}
