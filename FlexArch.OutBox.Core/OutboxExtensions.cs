using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Core.BackgroundServices;
using FlexArch.OutBox.Core.HealthChecks;
using FlexArch.OutBox.Core.MetricsReporters;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.DependencyInjection;

namespace FlexArch.OutBox.Core;

public static class OutboxExtensions
{
    public static IServiceCollection AddOutbox(this IServiceCollection services,
        Action<OutboxOptions>? outboxConfigure = null,
        Action<OutboxCleanupOptions>? outboxCleanupConfigure = null,
        Action<DeadLetterCleanupOptions>? deadLetterCleanupConfigure = null)
    {
        Action<OutboxOptions> _outboxConfigure = outboxConfigure ?? (_ => { });

        // 配置核心OutBox选项
        services.Configure(_outboxConfigure);

        services.AddHostedService<OutboxProcessor>();

        if (outboxCleanupConfigure != null)
        {
            services.Configure(outboxCleanupConfigure);
            services.AddHostedService<OutboxCleanuper>();
        }

        if (deadLetterCleanupConfigure != null)
        {
            services.Configure(deadLetterCleanupConfigure);
            services.AddHostedService<DeadLetterCleanuper>();
        }

        return services;
    }

    public static IServiceCollection AddOutboxMiddleware<T>(this IServiceCollection services) where T : class, IOutboxMiddleware
    {
        services.AddSingleton<IOutboxMiddleware, T>();
        return services;
    }

    /// <summary>
    /// 添加OutBox健康检查
    /// </summary>
    public static IServiceCollection AddOutboxHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<OutboxHealthCheck>("outbox", tags: new[] { "outbox", "messaging" })
            .AddCheck<DeadLetterHealthCheck>("dead_letter", tags: new[] { "dead_letter", "messaging" });

        return services;
    }

    public static IServiceCollection AddConsoleMetricsReporter(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsReporter, ConsoleMetricsReporter>();
        return services;
    }

    public static IServiceCollection AddOpenTelemetryMetricsReporter(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsReporter, OpenTelemetryMetricsReporter>();
        return services;
    }
}
