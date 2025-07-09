using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Abstractions.IStores;

namespace FlexArch.OutBox.Persistence.EFCore;

/// <summary>
/// 扩展的OutboxStore接口，提供额外的管理功能
/// 主要用于测试、示例和管理场景
/// </summary>
public interface IExtendedOutboxStore : IOutboxStore
{
    /// <summary>
    /// 获取待处理的消息列表
    /// </summary>
    public Task<IReadOnlyList<IOutboxMessage>> GetPendingMessagesAsync(int maxCount = 100);

    /// <summary>
    /// 标记消息为已处理
    /// </summary>
    public Task MarkAsProcessedAsync(Guid messageId);

    /// <summary>
    /// 标记消息为失败
    /// </summary>
    public Task MarkAsFailedAsync(Guid messageId, string errorReason);

    /// <summary>
    /// 删除指定消息
    /// </summary>
    public Task DeleteAsync(Guid messageId);

    /// <summary>
    /// 删除指定时间之前的已处理消息
    /// </summary>
    public Task<int> DeleteProcessedBeforeAsync(DateTime cutoff);
}
