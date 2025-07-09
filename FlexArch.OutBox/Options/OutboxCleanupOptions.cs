namespace FlexArch.OutBox.Core.Options;

public class OutboxCleanupOptions
{
    public int RetainDays { get; set; } = 7;            // 保留最近几天的消息
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);  // 清理频率
}
