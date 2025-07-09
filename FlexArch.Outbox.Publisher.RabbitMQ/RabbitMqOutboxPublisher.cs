using System.Text;
using System.Text.Json;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FlexArch.Outbox.Publisher.RabbitMQ;

/// <summary>
/// 基于 RabbitMQ 的 OutBox 消息发布器实现
/// </summary>
public class RabbitMqOutboxPublisher : IOutboxPublisher
{
    private readonly RabbitMqConnectionManager _connectionManager;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;

    /// <summary>
    /// 初始化 RabbitMqOutboxPublisher 实例
    /// </summary>
    /// <param name="connectionManager">RabbitMQ 连接管理器</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException">当任何参数为 null 时抛出</exception>
    public RabbitMqOutboxPublisher(RabbitMqConnectionManager connectionManager, ILogger<RabbitMqOutboxPublisher> logger)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 异步发布消息到 RabbitMQ
    /// </summary>
    /// <param name="outboxMessage">要发布的 OutBox 消息</param>
    /// <returns>发布操作的任务</returns>
    /// <exception cref="ArgumentNullException">当 outboxMessage 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当消息类型或内容无效时抛出</exception>
    public async Task PublishAsync(IOutboxMessage outboxMessage)
    {
        ArgumentNullException.ThrowIfNull(outboxMessage);

        if (string.IsNullOrWhiteSpace(outboxMessage.Type))
        {
            throw new ArgumentException("Message type cannot be null or empty", nameof(outboxMessage));
        }

        if (string.IsNullOrWhiteSpace(outboxMessage.Payload))
        {
            throw new ArgumentException("Message payload cannot be null or empty", nameof(outboxMessage));
        }

        IConnection connection = await _connectionManager.GetConnectionAsync();
        using IChannel channel = await connection.CreateChannelAsync();
        byte[] body = Encoding.UTF8.GetBytes(outboxMessage.Payload);

        Dictionary<string, object?> headers = ConvertHeadersForRabbitMq(outboxMessage.Headers);
        headers["MessageType"] = outboxMessage.Type;
        var props = new BasicProperties
        {
            Headers = headers,
            MessageId = outboxMessage.Id.ToString(),
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
        exchange: "",
        routingKey: outboxMessage.Type,
        mandatory: false,
        basicProperties: props,
        body: body);

        _logger.LogInformation("[Outbox] Published message {Id} to queue {Type}", outboxMessage.Id, outboxMessage.Type);
    }

    /// <summary>
    /// 转换Headers为RabbitMQ支持的类型
    /// RabbitMQ不支持JsonElement等复杂类型，需要转换为基本类型
    /// </summary>
    private static Dictionary<string, object?> ConvertHeadersForRabbitMq(IDictionary<string, object?>? headers)
    {
        if (headers == null || headers.Count == 0)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>();
        foreach (KeyValuePair<string, object?> kvp in headers)
        {
            result[kvp.Key] = ConvertValueForRabbitMq(kvp.Value);
        }
        return result;
    }

    /// <summary>
    /// 转换单个值为RabbitMQ支持的类型
    /// </summary>
    private static object? ConvertValueForRabbitMq(object? value)
    {
        return value switch
        {
            null => null,
            JsonElement jsonElement => ConvertJsonElementToString(jsonElement),
            string s => s,
            bool b => b,
            byte by => by,
            sbyte sb => sb,
            short sh => sh,
            ushort ush => ush,
            int i => i,
            uint ui => ui,
            long l => l,
            ulong ul => ul,
            float f => f,
            double d => d,
            decimal de => de,
            byte[] bytes => bytes,
            // 对于其他复杂类型，转换为字符串
            _ => value.ToString()
        };
    }

    /// <summary>
    /// 将JsonElement转换为字符串
    /// </summary>
    private static string ConvertJsonElementToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "",
            _ => element.GetRawText()
        };
    }
}
