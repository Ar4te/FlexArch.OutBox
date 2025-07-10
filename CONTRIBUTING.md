# 贡献指南

感谢您对 FlexArch.OutBox 项目的关注！我们欢迎所有形式的贡献，包括代码、文档、问题报告和功能建议。

## 🤝 如何贡献

### 贡献类型

- 🐛 **Bug 修复** - 修复项目中的错误
- ✨ **新功能** - 添加新的功能特性
- 📚 **文档改进** - 完善文档和示例
- 🔧 **性能优化** - 提升代码性能
- ✅ **测试** - 增加测试覆盖率
- 🎨 **代码质量** - 重构和代码优化

## 🚀 快速开始

### 1. 开发环境设置

#### 必需工具

- **.NET 8.0 SDK** 或更高版本
- **Visual Studio 2022** 或 **VS Code** 或 **JetBrains Rider**
- **Git** 版本控制
- **Docker** (可选，用于运行依赖服务)

#### 推荐工具

- **RabbitMQ** (本地测试)
- **SQL Server** / **PostgreSQL** / **MySQL** (数据库测试)
- **Postman** (API 测试)

### 2. 克隆和构建项目

```bash
# 1. Fork 项目到您的账户
# 2. 克隆您的 fork
git clone https://github.com/YOUR-USERNAME/FlexArch.OutBox.git
cd FlexArch.OutBox

# 3. 添加上游仓库
git remote add upstream https://github.com/ORIGINAL-OWNER/FlexArch.OutBox.git

# 4. 安装依赖和构建
dotnet restore
dotnet build

# 5. 运行测试
dotnet test
```

### 3. 创建功能分支

```bash
# 从最新的 main 分支创建功能分支
git checkout main
git pull upstream main
git checkout -b feature/your-feature-name

# 或者创建 bug 修复分支
git checkout -b fix/bug-description
```

## 📋 代码贡献流程

### 1. 开发准备

#### 分支命名规范

- `feature/功能名称` - 新功能开发
- `fix/问题描述` - Bug 修复
- `docs/文档改进` - 文档更新
- `test/测试改进` - 测试相关
- `refactor/重构描述` - 代码重构

#### 开发前检查

- [ ] 确认 issue 存在且已被接受
- [ ] 理解需求和技术方案
- [ ] 检查是否有相关的 PR 正在进行
- [ ] 与维护者讨论复杂的功能设计

### 2. 编码规范

#### C# 代码风格

```csharp
// ✅ 好的示例
public class OutboxService : IOutboxService
{
    private readonly IOutboxStore _outboxStore;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(
        IOutboxStore outboxStore,
        ILogger<OutboxService> logger)
    {
        _outboxStore = outboxStore ?? throw new ArgumentNullException(nameof(outboxStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ProcessMessageAsync(IOutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            _logger.LogInformation("Processing outbox message {MessageId} of type {MessageType}",
                message.Id, message.Type);

            // 实现逻辑...

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            throw;
        }
    }
}
```

#### 编码标准

- **命名约定**: 使用 PascalCase 命名类和方法，camelCase 命名参数和局部变量
- **异步方法**: 所有异步方法必须以 `Async` 结尾
- **参数验证**: 使用 `ArgumentNullException.ThrowIfNull()` 进行参数验证
- **日志记录**: 使用结构化日志，包含必要的上下文信息
- **异常处理**: 适当的异常处理和资源清理
- **取消令牌**: 长时间运行的操作必须支持 `CancellationToken`

### 3. 测试要求

#### 单元测试

```csharp
[Fact]
public async Task ProcessMessageAsync_WithValidMessage_ShouldReturnTrue()
{
    // Arrange
    var message = new OutboxMessage
    {
        Id = Guid.NewGuid(),
        Type = "TestMessage",
        Payload = "{\"data\":\"test\"}"
    };

    var mockStore = new Mock<IOutboxStore>();
    var mockLogger = new Mock<ILogger<OutboxService>>();
    var service = new OutboxService(mockStore.Object, mockLogger.Object);

    // Act
    var result = await service.ProcessMessageAsync(message);

    // Assert
    Assert.True(result);
    mockStore.Verify(x => x.UpdateStatusAsync(message.Id, OutboxStatus.Processed), Times.Once);
}

[Fact]
public async Task ProcessMessageAsync_WithNullMessage_ShouldThrowArgumentNullException()
{
    // Arrange
    var service = new OutboxService(Mock.Of<IOutboxStore>(), Mock.Of<ILogger<OutboxService>>());

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(() => service.ProcessMessageAsync(null!));
}
```

#### 测试覆盖要求

- **新功能**: 必须包含完整的单元测试
- **Bug 修复**: 必须包含重现 bug 的测试
- **覆盖率**: 新代码的测试覆盖率应 ≥ 80%
- **集成测试**: 复杂功能需要集成测试

### 4. 文档要求

#### XML 文档注释

```csharp
/// <summary>
/// 处理 OutBox 消息的核心服务
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// 异步处理指定的 OutBox 消息
    /// </summary>
    /// <param name="message">要处理的 OutBox 消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>如果处理成功返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentNullException">当 message 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当消息状态无效时抛出</exception>
    Task<bool> ProcessMessageAsync(IOutboxMessage message, CancellationToken cancellationToken = default);
}
```

#### README 更新

如果您的贡献影响了用户使用方式，请更新：

- 安装指南
- 配置说明
- 使用示例
- API 文档

## 🐛 问题报告

### Bug 报告模板

```markdown
## Bug 描述

简要描述遇到的问题

## 复现步骤

1. 执行操作 A
2. 配置参数 B
3. 观察到错误 C

## 期望行为

描述期望的正确行为

## 实际行为

描述实际观察到的行为

## 环境信息

- .NET 版本: 8.0
- FlexArch.OutBox 版本: 1.0.0-alpha.1
- 操作系统: Windows 11 / Linux / macOS
- 数据库: SQL Server 2022
- 消息队列: RabbitMQ 3.12

## 附加信息

- 错误日志
- 配置文件
- 测试代码
```

### 功能请求模板

```markdown
## 功能描述

简要描述建议的新功能

## 使用场景

描述在什么情况下需要这个功能

## 建议的解决方案

描述您认为应该如何实现这个功能

## 替代方案

描述其他可能的实现方式

## 附加上下文

提供任何其他相关信息
```

## 🔄 Pull Request 流程

### 1. 提交前检查清单

- [ ] 代码遵循项目编码规范
- [ ] 所有测试通过 (`dotnet test`)
- [ ] 新功能有完整的测试覆盖
- [ ] 文档已更新（如需要）
- [ ] commit 消息遵循规范
- [ ] 没有合并冲突

### 2. Commit 消息规范

```bash
# 格式: <类型>(<范围>): <描述>

# 类型
feat: 新功能
fix: Bug 修复
docs: 文档更新
style: 代码格式调整
refactor: 代码重构
test: 测试相关
chore: 构建过程或辅助工具的变动

# 示例
feat(middleware): 添加消息签名中间件
fix(efcore): 修复 JsonElement 序列化问题
docs(readme): 更新安装指南
test(outbox): 增加 OutboxProcessor 单元测试
```

### 3. PR 描述模板

```markdown
## 变更概述

简要描述这个 PR 的目的和实现

## 变更类型

- [ ] Bug 修复
- [ ] 新功能
- [ ] 破坏性变更
- [ ] 文档更新
- [ ] 性能优化
- [ ] 其他: \_\_\_

## 测试

- [ ] 新增了单元测试
- [ ] 新增了集成测试
- [ ] 所有现有测试通过
- [ ] 手动测试通过

## 文档

- [ ] 更新了 README
- [ ] 更新了 API 文档
- [ ] 更新了示例代码
- [ ] 不需要文档更新

## 检查清单

- [ ] 代码遵循项目规范
- [ ] 自测通过
- [ ] 准备好接受代码审查
```

## 👥 代码审查

### 审查重点

1. **功能正确性** - 代码是否正确实现了需求
2. **代码质量** - 是否遵循最佳实践和编码规范
3. **测试覆盖** - 是否有足够的测试覆盖
4. **性能影响** - 是否对性能有负面影响
5. **安全性** - 是否存在安全隐患
6. **向后兼容** - 是否破坏了现有 API

### 审查流程

1. **自动检查** - CI 系统会自动运行测试和代码质量检查
2. **人工审查** - 维护者会仔细审查代码变更
3. **反馈和修改** - 根据审查意见进行修改
4. **最终批准** - 获得批准后合并到主分支

## 🏗️ 项目架构

### 项目结构

```
FlexArch.OutBox/
├── FlexArch.OutBox.Abstractions/     # 核心抽象和接口
├── FlexArch.OutBox.Core/            # 核心实现
├── FlexArch.OutBox.EFCore/          # EF Core 持久化
├── FlexArch.OutBox.Publisher.RabbitMQ/ # RabbitMQ 发布器
├── FlexArch.OutBox.Tests/           # 单元测试
├── FlexArch.OutBox.TestAPI/         # 测试 API
└── examples/                        # 示例项目
```

### 设计原则

- **依赖倒置** - 依赖抽象而非具体实现
- **单一职责** - 每个类只负责一个功能
- **开闭原则** - 对扩展开放，对修改关闭
- **中间件模式** - 使用管道模式处理消息
- **异步优先** - 所有 I/O 操作都是异步的

## 🎯 开发指南

### 添加新的中间件

```csharp
public class YourCustomMiddleware : IOutboxMiddleware
{
    private readonly ILogger<YourCustomMiddleware> _logger;

    public YourCustomMiddleware(ILogger<YourCustomMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next)
    {
        _logger.LogInformation("Processing message {MessageId} with custom middleware", message.Id);

        try
        {
            // 前置处理
            await PreProcessAsync(message);

            // 调用下一个中间件
            await next(message);

            // 后置处理
            await PostProcessAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom middleware failed for message {MessageId}", message.Id);
            throw;
        }
    }

    private Task PreProcessAsync(IOutboxMessage message)
    {
        // 实现前置逻辑
        return Task.CompletedTask;
    }

    private Task PostProcessAsync(IOutboxMessage message)
    {
        // 实现后置逻辑
        return Task.CompletedTask;
    }
}

// 扩展方法
public static class MiddlewareExtensions
{
    public static OutboxBuilder WithYourCustomMiddleware(this OutboxBuilder builder)
    {
        return builder.WithMiddleware<YourCustomMiddleware>();
    }
}
```

### 添加新的发布器

```csharp
public class YourCustomPublisher : IOutboxPublisher
{
    public async Task PublishAsync(IOutboxMessage outboxMessage)
    {
        // 实现您的发布逻辑
        await SendToYourMessageQueue(outboxMessage);
    }

    private async Task SendToYourMessageQueue(IOutboxMessage message)
    {
        // 实现具体的发送逻辑
    }
}

// 扩展方法
public static class PublisherExtensions
{
    public static OutboxBuilder AddYourCustomPublisher(
        this OutboxBuilder builder,
        Action<YourCustomOptions> configure)
    {
        builder.Services.Configure(configure);
        builder.Services.AddScoped<IOutboxPublisher, YourCustomPublisher>();
        return builder;
    }
}
```

## 📞 获取帮助

### 联系方式

- **GitHub Issues** - 报告 bug 或请求功能
- **GitHub Discussions** - 讨论和询问问题
- **Code Review** - 通过 PR 获取代码反馈

### 有用的资源

- [.NET 编码约定](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit 测试最佳实践](https://xunit.net/docs/getting-started)
- [Entity Framework Core 文档](https://docs.microsoft.com/en-us/ef/core/)
- [RabbitMQ .NET 客户端指南](https://www.rabbitmq.com/dotnet.html)

## 🙏 感谢

感谢每一位贡献者！您的贡献让 FlexArch.OutBox 变得更好。

特别感谢：

- 所有提交代码的开发者
- 报告问题的用户
- 改进文档的贡献者
- 在社区中帮助他人的志愿者

---

**再次感谢您对 FlexArch.OutBox 项目的贡献！** 🎉
