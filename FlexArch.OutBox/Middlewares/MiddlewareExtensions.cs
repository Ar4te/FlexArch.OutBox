using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Core.Options;
using FlexArch.OutBox.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FlexArch.OutBox.Core.Middlewares;

public static class MiddlewareExtensions
{
    /// <summary>
    /// It should register at first.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection WithDelay(this IServiceCollection services)
    {
        services.AddOutboxMiddleware<DelayUntilMiddleware>();

        return services;
    }

    public static IServiceCollection WithMetrics(this IServiceCollection services)
    {
        services.AddOutboxMiddleware<MetricsMiddleware>();

        return services;
    }

    public static IServiceCollection WithTracing(this IServiceCollection services)
    {
        services.AddOutboxMiddleware<TracingMiddleware>();

        return services;
    }

    public static IServiceCollection WithRetry(this IServiceCollection services, Action<RetryOptions> retryConfigure)
    {
        services.Configure(retryConfigure);
        services.AddOutboxMiddleware<RetryMiddleware>();

        return services;
    }

    public static IServiceCollection WithCriticalBreaker(this IServiceCollection services, Action<CircuitBreakerOptions> circuitBreakerConfigure)
    {
        services.Configure(circuitBreakerConfigure);
        services.AddOutboxMiddleware<CircuitBreakerMiddleware>();

        return services;
    }

    public static IServiceCollection WithMessageSigning(this IServiceCollection services, Action<SigningOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<ISignatureProvider>(sp =>
        {
            IOptions<SigningOptions> options = sp.GetRequiredService<IOptions<SigningOptions>>();
            options.Value.Validate(); // 验证配置

            if (!options.Value.EnableSigning)
            {
                // 如果禁用签名，返回空实现
                return new NoOpSignatureProvider();
            }

            return new HmacSha256SignatureProvider(options.Value.SecretKey);
        });

        services.AddOutboxMiddleware<MessageSigningMiddleware>();
        return services;
    }

    public static IServiceCollection WithDeadLetter(this IServiceCollection services)
    {
        services.AddOutboxMiddleware<DeadLetterMiddleware>();
        return services;
    }
}
