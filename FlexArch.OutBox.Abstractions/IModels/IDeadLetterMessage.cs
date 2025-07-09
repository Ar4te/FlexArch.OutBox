namespace FlexArch.OutBox.Abstractions.IModels;

public interface IDeadLetterMessage
{
    public Guid Id { get; }
    public string Type { get; }
    public string Payload { get; }
    public DateTime CreatedAt { get; }
    public DateTime FailedAt { get; }
    public string? ErrorReason { get; }
    public IDictionary<string, object?> Headers { get; }
}
