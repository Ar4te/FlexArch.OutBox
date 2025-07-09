using System.Diagnostics.Metrics;
using FlexArch.OutBox.Abstractions;

namespace FlexArch.OutBox.Core.MetricsReporters;

public class OpenTelemetryMetricsReporter : IMetricsReporter
{
    private static readonly Meter Meter = new("Outbox", "1.0.0");

    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("outbox.event.duration", "ms");
    private static readonly Counter<long> SuccessCounter = Meter.CreateCounter<long>("outbox.event.success");
    private static readonly Counter<long> FailureCounter = Meter.CreateCounter<long>("outbox.event.failure");

    public void RecordSuccess(string eventType, long elapsedMilliseconds)
    {
        Duration.Record(elapsedMilliseconds, KeyValuePair.Create<string, object?>("event_type", eventType), KeyValuePair.Create<string, object?>("status", "success"));
        SuccessCounter.Add(1, KeyValuePair.Create("event_type", (object?)eventType));
    }

    public void RecordFailure(string eventType, long elapsedMilliseconds, Exception ex)
    {
        Duration.Record(elapsedMilliseconds, KeyValuePair.Create<string, object?>("event_type", eventType), KeyValuePair.Create<string, object?>("status", "failure"));
        FailureCounter.Add(1, KeyValuePair.Create("event_type", (object?)eventType));
    }
}
