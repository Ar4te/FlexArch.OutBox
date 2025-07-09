using System.Globalization;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Core.Middlewares;

public class DelayUntilMiddleware : IOutboxMiddleware
{
    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        if (message.Headers != null &&
            message.Headers.TryGetValue(MessageHeaderNames.DelayUntil, out object? delayUntilObj) &&
            delayUntilObj != null)
        {
            string? delayUntilStr = delayUntilObj.ToString();
            if (!string.IsNullOrEmpty(delayUntilStr) &&
                DateTime.TryParse(delayUntilStr, null, DateTimeStyles.AdjustToUniversal, out DateTime delayUntil))
            {
                if (DateTime.UtcNow < delayUntil)
                {
                    // 消息还未到延迟发送时间，跳过此次处理
                    return;
                }
            }
        }

        await next(message);
    }
}
