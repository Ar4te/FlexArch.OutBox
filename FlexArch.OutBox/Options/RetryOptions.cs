namespace FlexArch.OutBox.Core.Options;

public class RetryOptions
{
    public int MaxRetryCount { get; set; } = 3;
    public int DelayInSeconds { get; set; } = 2;
}
