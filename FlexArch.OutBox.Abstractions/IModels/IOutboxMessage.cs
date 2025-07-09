namespace FlexArch.OutBox.Abstractions.IModels;

/// <summary>
/// 保持抽象，具体实现可用 EFCore 实体，也可用于 MongoDB、Dapper 实现
/// </summary>
public interface IOutboxMessage
{
    public Guid Id { get; }
    public string Type { get; }
    public string Payload { get; }
    public DateTime CreatedAt { get; }
    public OutboxStatus Status { get; }
    public DateTime? SentAt { get; }
    public IDictionary<string, object?> Headers { get; set; }
}
