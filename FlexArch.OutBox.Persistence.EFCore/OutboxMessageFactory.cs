using System.Text.Json;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Persistence.EFCore.Models;

namespace FlexArch.OutBox.Persistence.EFCore;

public class OutboxMessageFactory
{
    public IOutboxMessage Create<T>(T payload, string type, Action<Dictionary<string, object?>>? configureHeaders = null)
    {
        string json = JsonSerializer.Serialize(payload, typeof(T));
        var headers = new Dictionary<string, object?>
        {
            ["CreatedAt"] = DateTime.UtcNow.ToString("o")
        };

        configureHeaders?.Invoke(headers);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = json,
            CreatedAt = DateTime.UtcNow,
            Headers = headers
        };
    }
}
