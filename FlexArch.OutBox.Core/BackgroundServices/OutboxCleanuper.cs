using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlexArch.OutBox.Core.BackgroundServices;

public class OutboxCleanuper : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanuper> _logger;
    private readonly OutboxCleanupOptions _options;

    public OutboxCleanuper(IServiceProvider sp, ILogger<OutboxCleanuper> logger, IOptions<OutboxCleanupOptions> options)
    {
        _serviceProvider = sp;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_options.CleanupInterval, stoppingToken);

            using IServiceScope scope = _serviceProvider.CreateScope();
            IOutboxStore store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            DateTime cutoff = DateTime.UtcNow.AddDays(-_options.RetainDays);

            int deleted = await store.DeleteSentBeforeAsync(cutoff);
            _logger.LogInformation("[OutboxCleanup] Deleted {Count} published messages older than {Cutoff}", deleted, cutoff);
        }
    }
}
