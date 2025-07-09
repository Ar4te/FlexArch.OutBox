using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Persistence.EFCore.Models;

/// <summary>
/// OutBox消息实体
/// </summary>
public class OutboxMessage : IOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
    public IDictionary<string, object?> Headers { get; set; } = new Dictionary<string, object?>();
}
