using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlexArch.OutBox.Core.BackgroundServices;

public class DeadLetterCleanuper : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DeadLetterCleanuper> _logger;
    private readonly DeadLetterCleanupOptions _options;

    public DeadLetterCleanuper(IServiceProvider sp, IOptions<DeadLetterCleanupOptions> opts, ILogger<DeadLetterCleanuper> logger)
    {
        _sp = sp;
        _logger = logger;
        _options = opts.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_options.CleanupInterval, stoppingToken);

            using IServiceScope scope = _sp.CreateScope();
            IDeadLetterStore store = scope.ServiceProvider.GetRequiredService<IDeadLetterStore>();
            DateTime cutoff = DateTime.UtcNow.AddDays(-_options.RetainDays);

            int deleted = await store.DeleteDeadLettersBeforeAsync(cutoff);
            _logger.LogInformation("[DeadLetterCleanup] Deleted {Count} messages older than {Cutoff}", deleted, cutoff);
        }
    }
}
