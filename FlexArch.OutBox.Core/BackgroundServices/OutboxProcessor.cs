using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlexArch.OutBox.Core.BackgroundServices;

/// <summary>
/// OutBox 消息处理器后台服务，负责定期处理未发送的 OutBox 消息
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval;

    /// <summary>
    /// 初始化 OutboxProcessor 实例
    /// </summary>
    /// <param name="serviceProvider">服务提供器，用于创建作用域服务</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">OutBox 配置选项</param>
    /// <exception cref="ArgumentNullException">当任何参数为 null 时抛出</exception>
    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger, IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(options);

        _interval = options.Value.ProcessingInterval;
    }

    /// <summary>
    /// 后台服务的主执行方法，定期处理 OutBox 消息
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>执行任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started with interval: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in outbox processing cycle");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    /// <summary>
    /// 处理一批 OutBox 消息的核心逻辑
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理任务</returns>
    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IOutboxStore store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        IOutboxPublisher publisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();
        IOptions<OutboxOptions> options = scope.ServiceProvider.GetRequiredService<IOptions<OutboxOptions>>();

        IReadOnlyList<IOutboxMessage> messages = [];

        try
        {
            messages = await store.FetchUnsentMessagesAsync(options.Value.BatchSize);

            if (options.Value.EnableVerboseLogging)
            {
                _logger.LogDebug("Fetched {Count} unsent messages for processing", messages.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch outbox messages");
            return; // 获取消息失败时直接返回，等待下次处理
        }

        if (messages.Count == 0)
        {
            return; // 没有消息需要处理
        }

        int processedCount = 0;
        int failedCount = 0;

        foreach (IOutboxMessage message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Processing cancelled, processed {ProcessedCount}/{TotalCount} messages",
                    processedCount, messages.Count);
                break;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(options.Value.MessageProcessingTimeout);

                await publisher.PublishAsync(message);
                await store.MarkAsSentAsync(message.Id);
                processedCount++;

                if (options.Value.EnableVerboseLogging)
                {
                    _logger.LogDebug("Successfully published message {MessageId} of type {MessageType}",
                        message.Id, message.Type);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Message processing cancelled for message {MessageId}", message.Id);
                break;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Message processing timeout for message {MessageId} of type {MessageType}",
                    message.Id, message.Type);
                failedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} of type {MessageType}",
                    message.Id, message.Type);
                failedCount++;
            }
        }

        if (processedCount > 0 || failedCount > 0)
        {
            _logger.LogInformation("Outbox processing completed: {ProcessedCount} processed, {FailedCount} failed, {TotalCount} total",
                processedCount, failedCount, messages.Count);
        }
    }
}
