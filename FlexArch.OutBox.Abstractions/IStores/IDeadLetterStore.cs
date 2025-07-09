using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions.IStores;

public interface IDeadLetterStore
{
    public Task SaveAsync(IOutboxMessage message, string errorReason);
    public Task<int> DeleteDeadLettersBeforeAsync(DateTime cutoff);
    public Task<int> CountAsync();
}
