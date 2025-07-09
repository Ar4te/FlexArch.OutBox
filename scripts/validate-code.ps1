# FlexArch.OutBox 代码验证脚本
param(
    [switch]$SkipTests,
    [switch]$SkipFormat,
    [switch]$SkipSecurity,
    [switch]$Verbose,
    [switch]$Help
)

# 颜色输出函数
function Write-Success($msg) { Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error($msg) { Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning($msg) { Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info($msg) { Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Step($msg) { Write-Host "🔄 $msg" -ForegroundColor Blue }

# 显示帮助
function Show-Help {
    Write-Host ""
    Write-Host "FlexArch.OutBox 代码验证脚本" -ForegroundColor Magenta
    Write-Host "用法: .\scripts\validate-code.ps1 [选项]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "选项:" -ForegroundColor Yellow
    Write-Host "  -SkipTests     跳过单元测试"
    Write-Host "  -SkipFormat    跳过代码格式检查"
    Write-Host "  -SkipSecurity  跳过安全扫描"
    Write-Host "  -Verbose       显示详细输出"
    Write-Host "  -Help          显示此帮助信息"
    Write-Host ""
}

if ($Help) {
    Show-Help
    exit 0
}

Write-Host ""
Write-Host "🚀 FlexArch.OutBox 代码质量验证" -ForegroundColor Magenta
Write-Host "================================" -ForegroundColor Magenta
Write-Host ""

$startTime = Get-Date
$allPassed = $true

# 1. 检查 .NET SDK
Write-Step "检查 .NET SDK..."
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion -like "8.*") {
        Write-Success "发现 .NET SDK $dotnetVersion"
    }
    else {
        Write-Warning "建议使用 .NET 8.x，当前版本: $dotnetVersion"
    }
}
catch {
    Write-Error "未找到 .NET SDK！请安装 .NET 8.0 SDK"
    $allPassed = $false
}

# 2. 恢复依赖项
if ($allPassed) {
    Write-Step "恢复 NuGet 依赖项..."
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Success "依赖项恢复成功"
    }
    else {
        Write-Error "依赖项恢复失败"
        $allPassed = $false
    }
}

# 3. 代码格式检查
if ($allPassed -and -not $SkipFormat) {
    Write-Step "检查代码格式..."
    $verbosity = if ($Verbose) { "diagnostic" } else { "quiet" }
    dotnet format --verify-no-changes --verbosity $verbosity
    if ($LASTEXITCODE -eq 0) {
        Write-Success "代码格式检查通过"
    }
    else {
        Write-Error "代码格式不符合规范！运行 'dotnet format' 来修复"
        $allPassed = $false
    }
}
elseif ($SkipFormat) {
    Write-Info "跳过代码格式检查"
}

# 4. 构建项目
if ($allPassed) {
    Write-Step "构建解决方案..."
    $verbosity = if ($Verbose) { "normal" } else { "quiet" }
    dotnet build --no-restore --configuration Release -p:TreatWarningsAsErrors=true --verbosity $verbosity
    if ($LASTEXITCODE -eq 0) {
        Write-Success "构建成功"
    }
    else {
        Write-Error "构建失败！请修复编译错误和警告"
        $allPassed = $false
    }
}

# 5. 运行测试
if ($allPassed -and -not $SkipTests) {
    Write-Step "运行单元测试..."
    $verbosity = if ($Verbose) { "detailed" } else { "quiet" }
    dotnet test --no-build --configuration Release --logger "console;verbosity=$verbosity"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "所有测试通过"
    }
    else {
        Write-Error "测试失败！请修复失败的测试"
        $allPassed = $false
    }
}
elseif ($SkipTests) {
    Write-Info "跳过测试"
}

# 6. 生成测试覆盖率
if ($allPassed -and -not $SkipTests) {
    Write-Step "生成测试覆盖率报告..."
    if (Test-Path "TestResults") {
        Remove-Item "TestResults" -Recurse -Force
    }
    dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults --logger "console;verbosity=quiet"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "测试覆盖率报告生成完成"
        Write-Info "覆盖率文件位置: TestResults/"
    }
    else {
        Write-Warning "覆盖率报告生成失败"
    }
}

# 7. 安全扫描
if (-not $SkipSecurity) {
    Write-Step "执行安全扫描..."
    
    Write-Info "检查已知安全漏洞..."
    $vulnerableOutput = dotnet list package --vulnerable --include-transitive 2>&1 | Out-String
    if ($vulnerableOutput -match "vulnerable") {
        Write-Error "发现安全漏洞！"
        if ($Verbose) {
            Write-Host $vulnerableOutput
        }
        $allPassed = $false
    }
    else {
        Write-Success "未发现安全漏洞"
    }
    
    Write-Info "检查过时的依赖包..."
    $outdatedOutput = dotnet list package --outdated 2>&1 | Out-String
    if ($outdatedOutput -match "Project") {
        Write-Warning "发现过时的依赖包，建议更新"
        if ($Verbose) {
            Write-Host $outdatedOutput
        }
    }
    else {
        Write-Success "所有依赖包都是最新的"
    }
}
else {
    Write-Info "跳过安全扫描"
}

# 8. 创建 NuGet 包
if ($allPassed) {
    Write-Step "创建 NuGet 包..."
    
    if (-not (Test-Path "packages")) {
        New-Item -ItemType Directory -Path "packages" | Out-Null
    }
    
    $projects = @(
        "FlexArch.OutBox.Abstractions/FlexArch.OutBox.Abstractions.csproj",
        "FlexArch.OutBox/FlexArch.OutBox.Core.csproj",
        "FlexArch.OutBox.EFCore/FlexArch.OutBox.Persistence.EFCore.csproj",
        "FlexArch.Outbox.Publisher.RabbitMQ/FlexArch.Outbox.Publisher.RabbitMQ.csproj"
    )
    
    $packageSuccess = $true
    foreach ($project in $projects) {
        if (Test-Path $project) {
            Write-Info "打包 $project..."
            dotnet pack $project --no-build --configuration Release --output packages --verbosity quiet
            if ($LASTEXITCODE -ne 0) {
                Write-Error "打包 $project 失败"
                $packageSuccess = $false
            }
        }
        else {
            Write-Warning "项目文件不存在: $project"
        }
    }
    
    if ($packageSuccess) {
        Write-Success "所有包创建成功"
        Write-Info "包位置: packages/"
    }
    else {
        Write-Error "包创建失败"
        $allPassed = $false
    }
}

# 总结
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "📊 验证结果总结" -ForegroundColor Magenta
Write-Host "=================" -ForegroundColor Magenta

if ($allPassed) {
    Write-Success "所有检查通过！代码已准备好提交 ✨"
    Write-Host ""
    Write-Info "✅ .NET SDK 检查"
    Write-Info "✅ 依赖项恢复"
    if (-not $SkipFormat) { Write-Info "✅ 代码格式检查" }
    Write-Info "✅ 项目构建"
    if (-not $SkipTests) { Write-Info "✅ 单元测试" }
    if (-not $SkipTests) { Write-Info "✅ 测试覆盖率" }
    if (-not $SkipSecurity) { Write-Info "✅ 安全扫描" }
    Write-Info "✅ NuGet 包创建"
}
else {
    Write-Error "部分检查失败！请修复问题后重试"
    exit 1
}

Write-Host ""
Write-Info "Total time: $([math]::Round($duration.TotalSeconds, 2)) seconds"
Write-Host "" 