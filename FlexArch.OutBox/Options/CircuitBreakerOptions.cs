namespace FlexArch.OutBox.Core.Options;

public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; }
    public int DurationOfBreakInSeconds { get; set; }
}

public class DeadLetterCleanupOptions
{
    public int RetainDays { get; set; } = 30;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
}
