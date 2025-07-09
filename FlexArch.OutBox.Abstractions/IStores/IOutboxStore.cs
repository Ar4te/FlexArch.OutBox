using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions.IStores;

/// <summary>
/// OutBox 消息存储接口，定义消息持久化的核心操作
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// 异步保存 OutBox 消息到存储中
    /// </summary>
    /// <param name="outboxMessage">要保存的 OutBox 消息</param>
    /// <returns>保存操作的任务</returns>
    /// <exception cref="ArgumentNullException">当 outboxMessage 为 null 时抛出</exception>
    public Task SaveAsync(IOutboxMessage outboxMessage);

    /// <summary>
    /// 异步获取未发送的消息列表，按创建时间排序
    /// </summary>
    /// <param name="maxCount">最大返回数量，默认100</param>
    /// <returns>未发送消息的只读列表</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 maxCount 小于等于 0 时抛出</exception>
    public Task<IReadOnlyList<IOutboxMessage>> FetchUnsentMessagesAsync(int maxCount = 100);

    /// <summary>
    /// 异步标记指定消息为已发送状态
    /// </summary>
    /// <param name="messageId">消息的唯一标识符</param>
    /// <returns>标记操作的任务</returns>
    /// <exception cref="ArgumentException">当 messageId 为空 Guid 时抛出</exception>
    /// <exception cref="InvalidOperationException">当找不到指定消息时抛出</exception>
    public Task MarkAsSentAsync(Guid messageId);

    /// <summary>
    /// 异步删除指定时间之前已发送的消息（用于数据清理）
    /// </summary>
    /// <param name="cutoff">截止时间，早于此时间的已发送消息将被删除</param>
    /// <returns>删除的消息数量</returns>
    public Task<int> DeleteSentBeforeAsync(DateTime cutoff);
}
