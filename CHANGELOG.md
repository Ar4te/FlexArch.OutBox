# 变更日志

所有此项目的重要变更都将记录在这个文件中。

格式基于 [保持变更日志](https://keepachangelog.com/zh-CN/1.0.0/)，并且此项目遵循 [语义化版本控制](https://semver.org/lang/zh-CN/)。

## [未发布]

### 计划功能

- Apache Kafka 支持
- Azure Service Bus 支持
- Amazon SQS 支持
- 更多数据库索引优化
- 性能基准测试套件

## [1.0.0-alpha.1] - 2025-07-09

### 修复

- 🔧 **RabbitMQ JsonElement 兼容性修复**
  - 修复 `RabbitMQ.Client.Exceptions.WireFormattingException: Value of type 'JsonElement' cannot appear as table value` 错误
  - 在 `RabbitMqOutboxPublisher` 中添加智能类型转换，确保消息头兼容 RabbitMQ
  - 在 EF Core 配置中优化 Headers 序列化/反序列化，避免 JsonElement 类型问题
  - 支持 JsonElement 到基本类型的自动转换

### 新增

- 🎯 完整的 OutBox 模式实现
- 🏗️ 基于 Clean Architecture 的分层设计
- 🔧 流畅的 API 设计和配置
- 🚀 高性能批量处理和数据库优化
- 🛡️ 生产就绪的可靠性特性
- 🔌 中间件管道架构
- 📦 多项目包结构

#### 核心功能

- **抽象层 (FlexArch.OutBox.Abstractions)**

  - 定义核心接口：`IOutboxStore`, `IOutboxPublisher`, `IOutboxMiddleware`
  - 消息状态管理：`OutboxStatus` 枚举
  - 消息头常量定义

- **核心实现 (FlexArch.OutBox.Core)**

  - 后台服务：`OutboxProcessor`, `OutboxCleanuper`, `DeadLetterCleanuper`
  - 中间件 pipeline：重试、熔断、延迟、签名、追踪、指标
  - 配置选项：`OutboxOptions`, `RetryOptions`, `CircuitBreakerOptions`
  - 健康检查集成

- **持久化 (FlexArch.OutBox.EntityFramework)**

  - Entity Framework Core 集成
  - 优化的数据库实体：`OutboxMessage`, `DeadLetterMessage`
  - 自动索引配置
  - 批量操作优化

- **消息发布 (FlexArch.OutBox.RabbitMQ)**
  - RabbitMQ 集成
  - 连接管理优化
  - 传输信息提供者

#### 中间件特性

- **重试中间件**: 指数退避重试策略
- **熔断中间件**: 故障隔离和自动恢复
- **延迟中间件**: 支持延迟消息发送
- **签名中间件**: HMAC-SHA256 消息签名验证
- **追踪中间件**: OpenTelemetry 分布式追踪
- **指标中间件**: 性能指标收集

#### 监控和观测

- OpenTelemetry 集成
- 结构化日志记录
- 健康检查端点
- 性能指标报告

#### 安全特性

- 消息签名和验证
- 防时序攻击的常数时间比较
- 配置化密钥管理

#### 性能优化

- 批量数据库操作
- 优化的数据库索引
- 异步处理管道
- 连接池管理

### 文档

- 📖 完整的 README.md
- 🚀 快速开始示例项目
- 📋 最佳实践指南
- 🔗 架构设计说明

### 测试

- ✅ 基础单元测试套件
- 🧪 中间件测试
- 📝 示例项目集成测试

### 包配置

- 🎁 NuGet 包配置
- 📄 MIT 许可证
- 📊 符号包支持
- 🔄 CI/CD 准备

---

## 版本说明

### Alpha 版本

当前为 Alpha 版本，主要特点：

- ✅ 核心功能完整可用
- ✅ 基础测试覆盖
- ✅ 生产级别代码质量
- ⚠️ API 可能有微小变化
- ⚠️ 文档持续完善中

### 稳定性承诺

- 所有公共 API 设计经过深思熟虑
- 向后兼容性将在 1.0 正式版后严格维护
- 重大变更将遵循语义化版本控制

### 支持的.NET 版本

- .NET 8.0+

### 支持的数据库

通过 Entity Framework Core 支持：

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
- �� Amazon SQS (计划中)
