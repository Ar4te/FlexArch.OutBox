# FlexArch.OutBox

[![NuGet](https://img.shields.io/nuget/v/FlexArch.OutBox.svg)](https://www.nuget.org/packages/FlexArch.OutBox/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-blue)](https://dotnet.microsoft.com/)

**分层的 OutBox 分布式事务模式实现** - 专为.NET 微服务架构设计的轻量级、高性能分布式事务解决方案。

## ✨ 特性

- 🎯 **完整的 OutBox 模式实现** - 保证消息与业务数据的事务一致性
- 🏗️ **优雅的架构设计** - 基于 DDD 和 Clean Architecture 原则
- 🔧 **简单易用** - 流畅的 API 设计，3 行代码即可集成
- 🚀 **高性能** - 批量处理、数据库索引优化、异步处理
- 🛡️ **生产就绪** - 重试、熔断、健康检查、监控集成
- 🔌 **高度可扩展** - 中间件模式，支持自定义扩展
- 📦 **多存储支持** - Entity Framework Core（支持所有主流数据库）
- 🌐 **多传输支持** - RabbitMQ + 可扩展架构
- 📊 **完整监控** - OpenTelemetry、结构化日志、性能指标
- 🔧 **类型兼容** - 智能处理 JsonElement 等复杂类型，确保消息队列兼容性

## 🚀 快速开始

### 安装

```bash
# 核心包
dotnet add package FlexArch.OutBox.Core

# EF Core持久化支持
dotnet add package FlexArch.OutBox.Persistence.EFCore

# RabbitMQ消息发布
dotnet add package FlexArch.OutBox.Publisher.RabbitMQ
```

### 基础配置

```csharp
// Program.cs
builder.Services
    .AddOutbox(options =>
    {
        options.ProcessingInterval = TimeSpan.FromSeconds(10);
        options.BatchSize = 100;
    })
    .AddEfPersistence<YourDbContext>()
    .AddRabbitMqOutbox(connectionString => "amqp://localhost");

var app = builder.Build();
```

### 数据库配置

```csharp
// 在你的DbContext中
public class YourDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 一键应用OutBox配置（包括索引）
        modelBuilder.ApplyOutboxEntityConfigurations();

        // 你的其他实体配置...
    }
}
```

### 业务代码使用

```csharp
public class OrderService
{
    private readonly YourDbContext _context;
    private readonly IOutboxStore _outboxStore;

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. 保存业务数据
            var order = new Order(command.CustomerId, command.Items);
            _context.Orders.Add(order);

            // 2. 保存OutBox消息（不会立即提交）
            var outboxMessage = new OutboxMessage
            {
                Type = "OrderCreated",
                Payload = JsonSerializer.Serialize(new OrderCreatedEvent(order.Id))
            };
            await _outboxStore.SaveAsync(outboxMessage);

            // 3. 原子提交 - 保证一致性
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## 📚 详细文档

### 中间件配置

FlexArch.OutBox 提供了丰富的中间件来增强消息处理能力：

```csharp
builder.Services
    .AddOutbox()
    .AddEfPersistence<YourDbContext>()
    .AddRabbitMqOutbox(connectionString)

    // 重试机制
    .WithRetry(retry =>
    {
        retry.MaxRetryCount = 3;
        retry.DelayInSeconds = 2;
    })

    // 熔断器
    .WithCriticalBreaker(cb =>
    {
        cb.FailureThreshold = 5;
        cb.DurationOfBreakInSeconds = 30;
    })

    // 消息签名
    .WithMessageSigning(signing =>
    {
        signing.SecretKey = builder.Configuration["OutBox:SigningKey"];
        signing.EnableSigning = true;
    })

    // 延迟消息
    .WithDelay()

    // 分布式追踪
    .WithTracing()

    // 性能指标
    .WithMetrics()

    // 健康检查
    .AddOutboxHealthChecks();
```

### 配置选项

```csharp
builder.Services.Configure<OutboxOptions>(options =>
{
    options.ProcessingInterval = TimeSpan.FromSeconds(10);  // 处理间隔
    options.BatchSize = 100;                               // 批次大小
    options.MessageProcessingTimeout = TimeSpan.FromSeconds(30); // 超时时间
    options.EnableVerboseLogging = false;                  // 详细日志
});
```

### 支持的数据库

通过 Entity Framework Core，支持所有主流数据库：

- SQL Server
- PostgreSQL
- MySQL
- SQLite
- Oracle
- 等等...

### 支持的消息队列

- ✅ RabbitMQ
- 🔄 Apache Kafka (计划中)
- 🔄 Azure Service Bus (计划中)
- 🔄 Amazon SQS (计划中)

## 🏗️ 架构设计

### 包结构

```
FlexArch.OutBox.Abstractions           # 核心抽象接口
FlexArch.OutBox.Core                   # 核心实现
FlexArch.OutBox.Persistence.EFCore    # EF Core持久化
FlexArch.OutBox.Publisher.RabbitMQ    # RabbitMQ发布器
```

### 中间件管道

```
消息 → 延迟中间件 → 重试中间件 → 熔断中间件 → 签名中间件 → 指标中间件 → 追踪中间件 → 发布器
```

## 📊 监控和观测

### 健康检查

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### OpenTelemetry 集成

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder.AddSource("Outbox");
    });
```

### 性能指标

- `outbox.event.duration` - 消息处理耗时
- `outbox.event.success` - 成功处理计数
- `outbox.event.failure` - 失败处理计数

## 🛠️ 高级用法

### 自定义中间件

```csharp
public class CustomMiddleware : IOutboxMiddleware
{
    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        // 前置处理
        await next(message);
        // 后置处理
    }
}

// 注册
builder.Services.AddScoped<IOutboxMiddleware, CustomMiddleware>();
```

### 自定义发布器

```csharp
public class CustomPublisher : IOutboxPublisher
{
    public async Task PublishAsync(IOutboxMessage message)
    {
        // 自定义发布逻辑
    }
}

// 注册
builder.Services.AddScoped<IOutboxPublisher, CustomPublisher>();
```

## 📋 最佳实践

### 1. 事务边界控制

```csharp
// ✅ 正确 - 在业务事务中一起提交
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // 业务操作
    await _businessService.DoSomething();

    // OutBox消息
    await _outboxStore.SaveAsync(message);

    // 一起提交
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch { await transaction.RollbackAsync(); throw; }
```

### 2. 消息幂等性

```csharp
var message = new OutboxMessage
{
    Id = Guid.NewGuid(), // 确保唯一性
    Type = "OrderCreated",
    Payload = JsonSerializer.Serialize(orderEvent),
    Headers = new Dictionary<string, object?>
    {
        ["CorrelationId"] = correlationId,
        ["IdempotencyKey"] = idempotencyKey
    }
};
```

### 3. 错误处理

死信队列会自动处理失败的消息，无需手动干预。

## 🚧 故障排查

### 常见问题

**Q: 消息没有被处理？**
A: 检查 `OutboxProcessor` 后台服务是否正常运行，查看日志中是否有错误信息。

**Q: 数据库性能问题？**
A: 确保已应用推荐的数据库索引：`modelBuilder.ApplyOutboxEntityConfigurations()`

**Q: 消息重复发送？**
A: 检查消息消费端的幂等性实现，确保使用 `CorrelationId` 或 `IdempotencyKey`。

### 日志级别

```json
{
  "Logging": {
    "LogLevel": {
      "FlexArch.OutBox": "Information"
    }
  }
}
```

## 🤝 贡献

我们欢迎社区贡献！请查看我们的 [贡献指南](CONTRIBUTING.md) 了解如何：

- 🐛 报告问题和提交 Bug 修复
- ✨ 提议和实现新功能
- 📚 改进文档和示例
- 🧪 增加测试覆盖率

快速开始贡献：

```bash
# 1. Fork 并克隆项目
git clone https://github.com/YOUR-USERNAME/FlexArch.OutBox.git
cd FlexArch.OutBox

# 2. 创建功能分支
git checkout -b feature/your-feature-name

# 3. 构建和测试
dotnet build
dotnet test

# 4. 提交您的更改
git commit -m "feat: 添加您的功能描述"
git push origin feature/your-feature-name

# 5. 创建 Pull Request
```

## 📄 许可证

本项目采用 [MIT 许可证](LICENSE)。

## 🔗 相关链接

- [完整示例项目](examples/)
- [变更日志](CHANGELOG.md)
- [版本信息](VERSION_INFO.md)
- [发布总结](RELEASE_SUMMARY.md)

## ⭐ 如果这个项目对你有帮助，请给个星星！

---

**FlexArch.OutBox** - 让分布式事务变得简单可靠！
