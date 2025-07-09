using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Persistence.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexArch.OutBox.Persistence.EFCore.Stores;

/// <summary>
/// 基于 Entity Framework Core 的 OutBox 消息存储实现
/// </summary>
/// <typeparam name="TDbContext">数据库上下文类型</typeparam>
public class EfOutboxStore<TDbContext> : IExtendedOutboxStore where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    /// <summary>
    /// 初始化 EfOutboxStore 实例
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <exception cref="ArgumentNullException">当 dbContext 为 null 时抛出</exception>
    public EfOutboxStore(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task SaveAsync(IOutboxMessage outboxMessage)
    {
        ArgumentNullException.ThrowIfNull(outboxMessage);

        _dbContext.Set<OutboxMessage>().Add((OutboxMessage)outboxMessage);
        // 注意：这里不调用SaveChangesAsync()以保持事务一致性
        // 由调用方决定何时提交事务，通常与业务数据一起提交
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<IOutboxMessage>> FetchUnsentMessagesAsync(int maxCount = 100)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count must be greater than 0");
        }

        return await _dbContext.Set<OutboxMessage>()
            .Where(x => x.Status == OutboxStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task MarkAsSentAsync(Guid messageId)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        }

        OutboxMessage msg = await _dbContext.Set<OutboxMessage>().FindAsync(messageId)
            ?? throw new InvalidOperationException($"OutboxMessage with ID {messageId} not found");

        if (msg.Status == OutboxStatus.Processed)
        {
            // 消息已经标记为已处理，避免重复操作
            return;
        }

        msg.Status = OutboxStatus.Processed;
        msg.SentAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteSentBeforeAsync(DateTime cutoff)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.SentAt != null && m.SentAt < cutoff)
            .ExecuteDeleteAsync();
    }

    // 扩展方法，用于支持更详细的状态管理
    public async Task MarkAsFailedAsync(Guid messageId, string errorReason)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        }

        ArgumentNullException.ThrowIfNull(errorReason);

        OutboxMessage msg = await _dbContext.Set<OutboxMessage>().FindAsync(messageId)
            ?? throw new InvalidOperationException($"OutboxMessage with ID {messageId} not found");

        msg.Status = OutboxStatus.Failed;
        msg.RetryCount++;
        msg.LastError = errorReason;
        msg.SentAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid messageId)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        }

        OutboxMessage? msg = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
        if (msg != null)
        {
            _dbContext.Set<OutboxMessage>().Remove(msg);
            await _dbContext.SaveChangesAsync();
        }
    }

    // IExtendedOutboxStore 的额外方法
    public async Task<IReadOnlyList<IOutboxMessage>> GetPendingMessagesAsync(int maxCount = 100)
    {
        return await FetchUnsentMessagesAsync(maxCount);
    }

    public async Task MarkAsProcessedAsync(Guid messageId)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        }

        OutboxMessage msg = await _dbContext.Set<OutboxMessage>().FindAsync(messageId)
            ?? throw new InvalidOperationException($"OutboxMessage with ID {messageId} not found");

        if (msg.Status == OutboxStatus.Processed)
        {
            // 消息已经标记为已处理，避免重复操作
            return;
        }

        msg.Status = OutboxStatus.Processed;
        msg.SentAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime cutoff)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxStatus.Processed && m.SentAt != null && m.SentAt < cutoff)
            .ExecuteDeleteAsync();
    }
}
