using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions;

/// <summary>
/// 提供消息传输层信息（如类型、系统、目的地等）
/// 供 Tracing/Metrics 等中间件采集标准化标签使用
/// </summary>
public interface IMessageTransportInfoProvider
{
    public string GetTransportSystem();       // e.g. "rabbitmq", "kafka"
    public string GetDestination(IOutboxMessage message); // e.g. topic name or queue name
    public string GetDestinationKind();       // e.g. "queue", "topic"
}
