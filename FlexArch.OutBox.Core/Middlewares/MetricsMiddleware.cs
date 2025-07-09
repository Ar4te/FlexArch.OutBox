using System.Diagnostics;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Core.Middlewares;

public class MetricsMiddleware : IOutboxMiddleware
{
    private readonly IMetricsReporter _reporter;
    private readonly IMessageTransportInfoProvider _transport;

    public MetricsMiddleware(IMetricsReporter reporter, IMessageTransportInfoProvider transport)
    {
        _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(next);

        var sw = Stopwatch.StartNew();
        try
        {
            await next(message);
            sw.Stop();

            string name = $"{_transport.GetTransportSystem()}.{_transport.GetDestinationKind()}.{_transport.GetDestination(message)}";
            _reporter.RecordSuccess(name, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            string name = $"{_transport.GetTransportSystem()}.{_transport.GetDestinationKind()}.{_transport.GetDestination(message)}";
            _reporter.RecordFailure(name, sw.ElapsedMilliseconds, ex);
            throw;
        }
    }
}
