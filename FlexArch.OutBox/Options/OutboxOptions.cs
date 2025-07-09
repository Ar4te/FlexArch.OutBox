namespace FlexArch.OutBox.Core.Options;

/// <summary>
/// OutBox核心配置选项
/// </summary>
public class OutboxOptions
{
    /// <summary>
    /// 消息处理间隔，默认10秒
    /// </summary>
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 每次处理的最大消息数量，默认100
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 最大重试次数，默认3次
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 是否启用死信队列，默认启用
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// 消息处理超时时间，默认30秒
    /// </summary>
    public TimeSpan MessageProcessingTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 是否启用详细日志，默认关闭
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;
}
