using System.Diagnostics;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Core.Middlewares;

public class MessageSigningMiddleware : IOutboxMiddleware
{
    private readonly ISignatureProvider _signer;

    public MessageSigningMiddleware(ISignatureProvider signer)
    {
        _signer = signer ?? throw new ArgumentNullException(nameof(signer));
    }

    public Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(next);

        string signature = _signer.Sign(message.Payload);
        message.Headers ??= new Dictionary<string, object?>();
        message.Headers[MessageHeaderNames.Signature] = signature;
        message.Headers[MessageHeaderNames.TraceId] = Activity.Current?.TraceId.ToString() ?? string.Empty;
        return next(message);
    }
}
