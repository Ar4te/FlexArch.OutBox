namespace FlexArch.OutBox.Abstractions;

public enum OutboxStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Processed = 3
}
