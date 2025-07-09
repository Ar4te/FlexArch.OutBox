using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions;

/// <summary>
/// OutBox 消息发布器接口，定义消息发布到外部系统的操作
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// 异步发布 OutBox 消息到目标系统（如消息队列、HTTP 端点等）
    /// </summary>
    /// <param name="outboxMessage">要发布的 OutBox 消息</param>
    /// <returns>发布操作的任务</returns>
    /// <exception cref="ArgumentNullException">当 outboxMessage 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当消息类型或内容无效时抛出</exception>
    public Task PublishAsync(IOutboxMessage outboxMessage);
}
