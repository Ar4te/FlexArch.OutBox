using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Persistence.EFCore.Models;

/// <summary>
/// 死信消息实体
/// </summary>
public class DeadLetterMessage : IDeadLetterMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorReason { get; set; }
    public IDictionary<string, object?> Headers { get; set; } = new Dictionary<string, object?>();
}
