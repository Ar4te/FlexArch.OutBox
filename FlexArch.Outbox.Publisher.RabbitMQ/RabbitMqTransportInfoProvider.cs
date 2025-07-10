using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Publisher.RabbitMQ;

public class RabbitMqTransportInfoProvider : IMessageTransportInfoProvider
{
    public string GetTransportSystem() => "rabbitmq";
    public string GetDestination(IOutboxMessage message) => message.Type;
    public string GetDestinationKind() => "queue";
}
