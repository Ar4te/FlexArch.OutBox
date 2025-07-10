using FlexArch.OutBox.Publisher.RabbitMQ;
using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Core;
using FlexArch.OutBox.Core.Middlewares;
using FlexArch.OutBox.Examples.QuickStart.Models;
using FlexArch.OutBox.Examples.QuickStart.Services;
using FlexArch.OutBox.Persistence.EFCore;
using FlexArch.OutBox.Persistence.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置数据库上下文
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseInMemoryDatabase("QuickStartDemo");
    options.ConfigureWarnings(config => config.Ignore(InMemoryEventId.TransactionIgnoredWarning));
});

// 配置OutBox - 这是关键配置!
builder.Services
    .AddOutbox(options =>
    {
        options.ProcessingInterval = TimeSpan.FromSeconds(5);
        options.BatchSize = 50;
        options.EnableVerboseLogging = true;
    })
    .AddEfPersistence<OrderDbContext>()
            .AddRabbitMqOutbox(factory =>
    {
        factory.Uri = new Uri("amqp://admin:admin@192.168.31.220:5672/");
    })

    // 添加中间件
    .WithRetry(retry =>
    {
        retry.MaxRetryCount = 3;
        retry.DelayInSeconds = 2;
    })
    .WithMetrics()
    .WithTracing()
    .AddOutboxHealthChecks()
    .AddConsoleMetricsReporter()
    .AddOpenTelemetryMetricsReporter();

// 业务服务
builder.Services.AddScoped<OrderService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 健康检查端点
app.MapHealthChecks("/health");

// 确保数据库已创建
using (IServiceScope scope = app.Services.CreateScope())
{
    OrderDbContext context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// API端点示例
app.MapPost("/orders", async (CreateOrderRequest request, OrderService orderService) =>
{
    try
    {
        Guid orderId = await orderService.CreateOrderAsync(request);
        return Results.Ok(new { OrderId = orderId, Message = "订单已创建，消息将异步发送" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"创建订单失败: {ex.Message}");
    }
})
.WithName("CreateOrder")
.WithOpenApi()
.WithSummary("创建订单 - 演示OutBox模式")
.WithDescription("创建订单并通过OutBox模式确保消息和数据的事务一致性");

app.MapGet("/orders", async (OrderDbContext context) =>
{
    var orders = await context.Orders
        .Select(o => new
        {
            o.Id,
            o.CustomerId,
            o.TotalAmount,
            o.CreatedAt,
            ItemCount = o.Items.Count
        })
        .ToListAsync();

    return Results.Ok(orders);
})
.WithName("GetOrders")
.WithOpenApi();

app.MapGet("/outbox-messages", async (IExtendedOutboxStore outboxStore) =>
{
    IReadOnlyList<FlexArch.OutBox.Abstractions.IModels.IOutboxMessage> messages = await outboxStore.GetPendingMessagesAsync(100);
    return Results.Ok(messages.Select(m => new
    {
        m.Id,
        m.Type,
        m.Status,
        m.CreatedAt,
        ProcessedAt = m.SentAt,
        RetryCount = (m as OutboxMessage)?.RetryCount ?? 0,
        PayloadPreview = m.Payload?.Length > 100 ? m.Payload[..100] + "..." : m.Payload
    }));
})
.WithName("GetOutboxMessages")
.WithOpenApi()
.WithSummary("查看OutBox消息状态")
.WithDescription("监控OutBox消息的处理状态");

app.MapDelete("/outbox-messages/processed", async (IExtendedOutboxStore outboxStore) =>
{
    // 这只是演示，生产环境中有自动清理机制
    IReadOnlyList<FlexArch.OutBox.Abstractions.IModels.IOutboxMessage> processed = await outboxStore.GetPendingMessagesAsync(1000);
    var processedMessages = processed.Where(m => m.Status == OutboxStatus.Processed).ToList();

    foreach (FlexArch.OutBox.Abstractions.IModels.IOutboxMessage? message in processedMessages)
    {
        await outboxStore.DeleteAsync(message.Id);
    }

    return Results.Ok(new { DeletedCount = processedMessages.Count });
})
.WithName("CleanupProcessedMessages")
.WithOpenApi();

Console.WriteLine("🚀 FlexArch.OutBox 快速开始示例已启动！");
Console.WriteLine("📖 访问 /swagger 查看API文档");
Console.WriteLine("❤️ 访问 /health 检查健康状态");
Console.WriteLine("📊 访问 /outbox-messages 监控消息状态");
Console.WriteLine("");
Console.WriteLine("💡 使用步骤：");
Console.WriteLine("1. POST /orders 创建订单");
Console.WriteLine("2. GET /outbox-messages 查看消息状态");
Console.WriteLine("3. 观察后台服务自动处理消息");

app.Run();
