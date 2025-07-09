using System.Diagnostics;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using OpenTelemetry.Trace;

namespace FlexArch.OutBox.Core.Middlewares;

public class TracingMiddleware : IOutboxMiddleware
{
    private readonly ActivitySource _activitySource;
    private readonly IMessageTransportInfoProvider _transport;

    public TracingMiddleware(IMessageTransportInfoProvider transport)
    {
        _activitySource = new ActivitySource("Outbox");
        _transport = transport;
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        Activity? activity = _activitySource.StartActivity($"Outbox Publish:{message.Type}", ActivityKind.Producer);

        if (activity != null)
        {
            activity.SetTag("messaging.system", _transport.GetTransportSystem());
            activity.SetTag("messaging.destination", _transport.GetDestination(message));
            activity.SetTag("messaging.destination_kind", _transport.GetDestinationKind());
            activity.SetTag("messaging.message_id", message.Id.ToString());
            activity.SetTag("message.payload_size", message.Payload.Length);
            activity.SetTag("messaging.operation", "publish");

            try
            {
                // Invoke the next middleware in the chain
                await next(message);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                throw;
            }
        }
        else
        {
            await next(message); // If no activity source, proceed normally
        }
    }
}
