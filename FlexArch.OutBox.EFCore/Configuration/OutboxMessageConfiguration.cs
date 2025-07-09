using FlexArch.OutBox.Persistence.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FlexArch.OutBox.EFCore.Configuration;

/// <summary>
/// OutboxMessage实体配置，实现IEntityTypeConfiguration以提供非侵入式配置
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        // 主键配置
        builder.HasKey(x => x.Id);

        // 属性配置
        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.SentAt)
            .IsRequired(false);

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        // Headers字段JSON序列化配置 - 确保反序列化后的值类型兼容RabbitMQ
        builder.Property(x => x.Headers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => DeserializeHeadersSafely(v)
            );

        // 性能优化索引
        builder.HasIndex(x => new { x.Status, x.CreatedAt }, "IX_OutboxMessage_Status_CreatedAt");
        builder.HasIndex(x => x.SentAt, "IX_OutboxMessage_SentAt");
        builder.HasIndex(x => x.Type, "IX_OutboxMessage_Type");
        builder.HasIndex(x => new { x.Status, x.SentAt }, "IX_OutboxMessage_Status_SentAt");
    }

    /// <summary>
    /// 安全地反序列化Headers，避免JsonElement类型问题
    /// </summary>
    private static Dictionary<string, object?> DeserializeHeadersSafely(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new Dictionary<string, object?>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var result = new Dictionary<string, object?>();

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                result[property.Name] = ConvertJsonElementToObject(property.Value);
            }

            return result;
        }
        catch
        {
            return new Dictionary<string, object?>();
        }
    }

    /// <summary>
    /// 将JsonElement转换为基本类型对象，避免RabbitMQ不支持的JsonElement类型
    /// </summary>
    private static object? ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int intValue) ? intValue :
                                   element.TryGetInt64(out long longValue) ? longValue :
                                   element.TryGetDouble(out double doubleValue) ? doubleValue : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElementToObject(p.Value)),
            _ => element.ToString()
        };
    }
}
