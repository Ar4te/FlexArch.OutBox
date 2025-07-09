using FlexArch.OutBox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace FlexArch.Outbox.Publisher.RabbitMQ;

public static class RabbitMqOutboxExtensions
{
    public static IServiceCollection AddRabbitMqOutbox(this IServiceCollection services, Action<IConnectionFactory> configure)
    {
        services.AddSingleton<IMessageTransportInfoProvider, RabbitMqTransportInfoProvider>();

        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var factory = new ConnectionFactory();
            configure(factory);
            return factory;
        });

        services.AddSingleton<RabbitMqConnectionManager>();

        services.AddSingleton<RabbitMqOutboxPublisher>();

        services.AddSingleton<IOutboxPublisher>(sp =>
        {
            RabbitMqOutboxPublisher rabbit = sp.GetRequiredService<RabbitMqOutboxPublisher>();
            IEnumerable<IOutboxMiddleware> middlewares = sp.GetServices<IOutboxMiddleware>();
            return new MiddlewareOutboxPublisher(middlewares, rabbit.PublishAsync);
        });

        return services;
    }
}
