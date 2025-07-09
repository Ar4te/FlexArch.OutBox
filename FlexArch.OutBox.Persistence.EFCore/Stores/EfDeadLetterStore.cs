using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Persistence.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexArch.OutBox.Persistence.EFCore.Stores;

public class EfDeadLetterStore<TDbContext> : IDeadLetterStore where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EfDeadLetterStore(TDbContext db)
    {
        _dbContext = db;
    }

    public async Task SaveAsync(IOutboxMessage message, string errorReason)
    {
        DeadLetterMessage deadLetterMessage = new()
        {
            Id = message.Id,
            Type = message.Type,
            Payload = message.Payload,
            CreatedAt = message.CreatedAt,
            Headers = message.Headers,
            ErrorReason = errorReason
        };

        _dbContext.Set<DeadLetterMessage>().Add(deadLetterMessage);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteDeadLettersBeforeAsync(DateTime cutoff)
    {
        return await _dbContext.Set<DeadLetterMessage>()
            .Where(d => d.FailedAt < cutoff)
            .ExecuteDeleteAsync();
    }

    public Task<int> CountAsync()
    {
        return _dbContext.Set<DeadLetterMessage>().CountAsync();
    }
}
