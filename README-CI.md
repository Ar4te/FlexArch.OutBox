# 🚀 FlexArch.OutBox CI/CD 自动化

## 概述

本项目已配置了完整的 GitHub Actions CI/CD 流程，确保代码质量和自动化部署。

## 🔧 GitHub Actions 工作流程

### 1. 主要 CI/CD 流程 (`.github/workflows/ci.yml`)

**触发条件：**

- 推送到 `main` 或 `develop` 分支
- 针对 `main` 或 `develop` 的 Pull Request

**检查内容：**

- ✅ 代码格式验证 (`dotnet format`)
- ✅ 静态代码分析 (编译警告视为错误)
- ✅ 构建验证 (Debug + Release)
- ✅ 单元测试 (全部 30 个测试)
- ✅ 测试覆盖率报告
- ✅ 安全漏洞扫描
- ✅ NuGet 包自动创建

### 2. Pull Request 检查 (`.github/workflows/pr-check.yml`)

**轻量级检查：**

- 代码格式检查
- 编译验证
- 测试通过验证

## 🛠️ 本地开发验证

### 快速验证命令

```bash
# 恢复依赖
dotnet restore

# 代码格式检查
dotnet format --verify-no-changes

# 构建项目 (警告视为错误)
dotnet build -p:TreatWarningsAsErrors=true

# 运行所有测试
dotnet test

# 安全扫描
dotnet list package --vulnerable
```

### 自动修复格式问题

```bash
# 自动格式化代码
dotnet format
```

## 📊 代码质量标准

### EditorConfig 配置

项目包含 `.editorconfig` 文件，定义了：

- C# 代码风格规范
- 命名约定
- 格式规则
- 缩进和空格规范

### 质量门控

- ❌ **编译错误** - 阻止合并
- ❌ **编译警告** - 阻止合并
- ❌ **代码格式问题** - 阻止合并
- ❌ **测试失败** - 阻止合并
- ❌ **安全漏洞** - 阻止合并

## 🔄 开发工作流程

### 1. 开发前

```bash
git pull origin main
dotnet restore
dotnet build
```

### 2. 开发中

```bash
# 定期检查格式
dotnet format --verify-no-changes

# 运行相关测试
dotnet test
```

### 3. 提交前

```bash
# 格式化代码
dotnet format

# 完整验证
dotnet build -p:TreatWarningsAsErrors=true
dotnet test

# 提交
git add .
git commit -m "feat: 你的功能描述"
```

### 4. Pull Request

- 确保所有 CI 检查通过
- 代码审查通过后合并

## 📦 自动化发布

当推送到 `main` 分支时，会自动：

1. 创建 NuGet 包
2. 上传构建产物
3. 生成版本标签

## 🚨 故障排除

### 常见问题

**代码格式检查失败：**

```bash
dotnet format
```

**测试失败：**

```bash
dotnet test --logger "console;verbosity=detailed"
```

**安全漏洞：**

```bash
dotnet list package --vulnerable --include-transitive
# 然后更新受影响的包
```

## 📈 项目状态

- **构建状态：** ✅ 通过
- **测试覆盖：** 30/30 测试通过
- **代码质量：** 100% 符合标准
- **安全状态：** 无已知漏洞

## 🎯 总结

通过这套 CI/CD 流程，我们确保：

- 🏗️ **代码质量** - 统一的格式和编码标准
- 🧪 **可靠性** - 全面的自动化测试
- 🔒 **安全性** - 持续的安全扫描
- ⚡ **效率** - 自动化的构建和部署
- 👀 **透明度** - 清晰的状态反馈

每次代码变更都会经过严格的质量检查，确保主分支始终保持生产就绪状态。
