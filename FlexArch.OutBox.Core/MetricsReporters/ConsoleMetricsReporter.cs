using FlexArch.OutBox.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlexArch.OutBox.Core.MetricsReporters;

public class ConsoleMetricsReporter : IMetricsReporter
{
    private readonly ILogger<ConsoleMetricsReporter> _logger;

    public ConsoleMetricsReporter(ILogger<ConsoleMetricsReporter> logger)
    {
        _logger = logger;
    }

    public void RecordSuccess(string eventType, long elapsedMilliseconds)
    {
        _logger.LogInformation("[Metrics] Success: {EventType} took {ElapsedMilliseconds}ms", eventType, elapsedMilliseconds);
    }

    public void RecordFailure(string eventType, long elapsedMilliseconds, Exception ex)
    {
        _logger.LogError(ex, "[Metrics] Failed: {EventType} took {ElapsedMilliseconds}ms. Exception: {Message}", eventType, elapsedMilliseconds, ex.Message);
    }
}
