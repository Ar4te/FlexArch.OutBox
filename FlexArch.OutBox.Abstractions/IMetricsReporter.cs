namespace FlexArch.OutBox.Abstractions;

public interface IMetricsReporter
{
    public void RecordSuccess(string eventType, long elapsedMilliseconds);

    public void RecordFailure(string eventType, long elapsedMilliseconds, Exception ex);
}
