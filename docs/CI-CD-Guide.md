# CI/CD 自动化指南

本指南介绍如何使用 GitHub Actions 为 FlexArch.OutBox 项目实现自动化的持续集成和持续部署。

## 🚀 概述

我们的 CI/CD 流程包含以下关键组件：

- **代码质量检查** - 格式、静态分析、编码规范
- **自动化测试** - 单元测试、集成测试、覆盖率检查
- **安全扫描** - 漏洞检测、依赖包安全性
- **自动化构建** - 多配置构建验证
- **包管理** - NuGet 包自动创建和发布

## 📋 工作流程说明

### 1. 主要 CI/CD 流程 (`.github/workflows/ci.yml`)

**触发条件**：

- 推送到 `main` 或 `develop` 分支
- 创建针对 `main` 或 `develop` 的 Pull Request

**包含的检查**：

#### 🔍 代码质量检查

```bash
# 代码格式验证
dotnet format --verify-no-changes

# 静态代码分析
dotnet build --warningsAsErrors
```

#### 🧪 构建和测试

```bash
# 多配置构建 (Debug + Release)
dotnet build --configuration Release

# 运行所有测试
dotnet test --collect:"XPlat Code Coverage"
```

#### 🔒 安全扫描

```bash
# 检查已知漏洞
dotnet list package --vulnerable

# 检查过时的包
dotnet list package --outdated
```

#### 📦 包构建

```bash
# 创建 NuGet 包（仅主分支）
dotnet pack --configuration Release
```

### 2. Pull Request 检查 (`.github/workflows/pr-check.yml`)

**触发条件**：创建或更新 Pull Request

这是一个轻量级的检查流程，确保 PR 符合基本质量要求：

- ✅ 代码格式正确
- ✅ 编译无错误和警告
- ✅ 所有测试通过
- ✅ 测试覆盖率符合要求

## 🛠️ 本地开发工具

### 代码格式化

```bash
# 检查代码格式
dotnet format --verify-no-changes

# 自动修复格式问题
dotnet format
```

### 运行所有检查（本地）

```bash
# 恢复依赖
dotnet restore

# 构建（将警告视为错误）
dotnet build --warningsAsErrors

# 运行测试
dotnet test

# 检查包漏洞
dotnet list package --vulnerable
```

## 📊 质量门控

### 构建门控

- ❌ **编译错误** - 阻止合并
- ❌ **编译警告** - 阻止合并（`--warningsAsErrors`）
- ❌ **代码格式问题** - 阻止合并
- ❌ **测试失败** - 阻止合并

### 安全门控

- ❌ **已知安全漏洞** - 阻止合并
- ⚠️ **过时的依赖包** - 警告但不阻止

### 测试门控

- ✅ **所有单元测试必须通过**
- ✅ **集成测试必须通过**
- 📊 **代码覆盖率报告**（可配置阈值）

## 🔧 配置和自定义

### 修改质量标准

编辑 `.editorconfig` 文件来调整代码格式规则：

```ini
# 示例：修改缩进大小
[*.cs]
indent_size = 4

# 示例：调整命名规则严重性
dotnet_naming_rule.private_fields_should_be_prefixed_with_underscore.severity = error
```

### 添加自定义检查

在 `.github/workflows/ci.yml` 中添加新的步骤：

```yaml
- name: 自定义质量检查
  run: |
    # 你的自定义检查命令
    echo "运行自定义检查..."
```

### 配置测试覆盖率阈值

```bash
# 示例：要求 80% 的代码覆盖率
dotnet test --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Threshold=80
```

## 📈 状态徽章

在 README.md 中添加构建状态徽章：

```markdown
[![CI/CD](https://github.com/your-username/FlexArch.OutBox/actions/workflows/ci.yml/badge.svg)](https://github.com/your-username/FlexArch.OutBox/actions/workflows/ci.yml)
[![Tests](https://github.com/your-username/FlexArch.OutBox/actions/workflows/pr-check.yml/badge.svg)](https://github.com/your-username/FlexArch.OutBox/actions/workflows/pr-check.yml)
```

## 🔄 发布流程

### 自动化包发布

当你创建一个以 `v` 开头的 Git 标签时，将自动触发 NuGet 包发布：

```bash
# 创建发布标签
git tag v1.0.0-alpha.2
git push origin v1.0.0-alpha.2
```

### 手动发布

```bash
# 构建包
dotnet pack --configuration Release --output ./packages

# 发布到 NuGet.org
dotnet nuget push ./packages/*.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## 🚨 故障排除

### 常见问题

#### 1. 代码格式检查失败

```bash
❌ 代码格式不符合规范！请运行 'dotnet format' 来修复格式问题。
```

**解决方案**：

```bash
dotnet format
git add .
git commit -m "fix: 修复代码格式问题"
```

#### 2. 测试失败

```bash
❌ 测试失败！请修复失败的测试。
```

**解决方案**：

1. 本地运行测试：`dotnet test --logger "console;verbosity=detailed"`
2. 修复失败的测试
3. 重新提交

#### 3. 安全漏洞检测

```bash
❌ 发现安全漏洞！
```

**解决方案**：

1. 查看漏洞详情：`dotnet list package --vulnerable --include-transitive`
2. 更新受影响的包：`dotnet add package PackageName --version NewVersion`
3. 重新测试和提交

### 调试工作流程

1. 查看 GitHub Actions 日志
2. 本地复现问题：
   ```bash
   # 模拟 CI 环境
   dotnet restore
   dotnet format --verify-no-changes
   dotnet build --warningsAsErrors
   dotnet test
   ```

## 📚 最佳实践

### 开发者工作流程

1. **开发前**：

   ```bash
   git pull origin main
   dotnet restore
   dotnet build
   ```

2. **开发中**：

   ```bash
   # 定期检查格式
   dotnet format --verify-no-changes

   # 运行相关测试
   dotnet test
   ```

3. **提交前**：

   ```bash
   # 完整检查
   dotnet format
   dotnet build --warningsAsErrors
   dotnet test
   git add .
   git commit -m "feat: 你的功能描述"
   ```

4. **创建 PR**：
   - 确保 PR 标题清晰描述改动
   - 检查 CI 状态通过后再请求审查

### 代码审查指南

审查者应检查：

- ✅ CI 检查全部通过
- ✅ 代码质量符合标准
- ✅ 测试覆盖新功能
- ✅ 文档更新（如需要）
- ✅ 没有引入安全风险

## 🎯 总结

通过这套 CI/CD 流程，我们确保：

- **代码质量**: 统一的格式和编码标准
- **可靠性**: 全面的自动化测试
- **安全性**: 持续的安全扫描
- **效率**: 自动化的构建和部署
- **透明度**: 清晰的状态反馈

每次代码变更都会经过严格的质量检查，确保主分支始终保持高质量和稳定性。
