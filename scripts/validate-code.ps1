# FlexArch.OutBox 验证脚本 - PowerShell 版
param(
    [switch]$SkipTests,
    [switch]$SkipFormat,
    [switch]$SkipSecurity,
    [switch]$SkipPack,
    [switch]$Verbose,
    [switch]$Help
)

function Write-Color { param([string]$Text, [string]$Color = "White"); Write-Host $Text -ForegroundColor $Color }
function Write-Step($msg) { Write-Color "🔄 $msg" Blue }
function Write-Info($msg) { Write-Color "ℹ️  $msg" Cyan }
function Write-Success($msg) { Write-Color "✅ $msg" Green }
function Write-WarningMsg($msg) { Write-Color "⚠️  $msg" Yellow }
function Write-Failure($msg) { Write-Color "❌ $msg" Red }

function Show-Help {
    Write-Host "`nFlexArch.OutBox 代码验证脚本" -ForegroundColor Magenta
    Write-Host "用法: .\validate-code.ps1 [选项]" -ForegroundColor Yellow
    Write-Host "`n选项:"
    Write-Host "  -SkipTests     跳过单元测试"
    Write-Host "  -SkipFormat    跳过代码格式检查"
    Write-Host "  -SkipSecurity  跳过安全扫描"
    Write-Host "  -SkipPack      跳过 NuGet 包打包"
    Write-Host "  -Verbose       显示详细输出"
    Write-Host "  -Help          显示此帮助信息"
    exit 0
}

if ($Help) { Show-Help }

Write-Host "`n🚀 FlexArch.OutBox 验证启动" -ForegroundColor Magenta
$ErrorActionPreference = "Stop"
$startTime = Get-Date
$solution = Get-ChildItem -Recurse -Filter *.sln | Select-Object -First 1
if (-not $solution) {
    Write-Failure "未找到解决方案文件 (*.sln)"
    exit 1
}

$solutionPath = $solution.FullName
$passed = $true

# SDK 检查
Write-Step ".NET SDK 检查..."
$version = dotnet --version
if ($version -like "8.*") {
    Write-Success ".NET SDK 版本 $version"
}
else {
    Write-WarningMsg "建议使用 .NET 8.x，当前为 $version"
}

# 恢复依赖
Write-Step "恢复依赖项..."
dotnet restore $solutionPath --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Failure "依赖项恢复失败"
    exit 1
}
Write-Success "依赖项恢复成功"

# 格式检查
if (-not $SkipFormat) {
    Write-Step "代码格式检查..."
    $verbosity = if ($Verbose) { "diagnostic" } else { "minimal" }
    dotnet format $solutionPath --verify-no-changes --verbosity $verbosity
    if ($LASTEXITCODE -eq 0) {
        Write-Success "代码格式检查通过"
    }
    else {
        Write-Failure "代码格式不规范，请执行 'dotnet format' 修复"
        $passed = $false
    }
}
else {
    Write-Info "已跳过代码格式检查"
}

# 构建项目
Write-Step "构建项目..."
dotnet build $solutionPath -c Release --no-restore -p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -eq 0) {
    Write-Success "项目构建成功"
}
else {
    Write-Failure "构建失败，请检查错误"
    $passed = $false
}

# 测试执行
if (-not $SkipTests) {
    Write-Step "运行测试..."
    dotnet test $solutionPath --no-build -c Release --logger "console;verbosity=detailed"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "测试全部通过"
    }
    else {
        Write-Failure "存在失败的测试"
        $passed = $false
    }

    Write-Step "生成测试覆盖率..."
    dotnet test --no-build -c Release --collect:"XPlat Code Coverage" --results-directory "TestResults"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "测试覆盖率生成完成"
        Write-Info "覆盖率结果: ./TestResults"
    }
    else {
        Write-WarningMsg "测试覆盖率生成失败"
    }
}
else {
    Write-Info "已跳过测试执行"
}

# 安全检查
if (-not $SkipSecurity) {
    Write-Step "进行安全扫描..."
    dotnet list package --vulnerable --include-transitive 2>&1 | Tee-Object -Variable vuln
    if ($vuln -match "vulnerable") {
        Write-Failure "发现安全漏洞！"
        if ($Verbose) { $vuln | Write-Host }
        $passed = $false
    }
    else {
        Write-Success "未发现已知安全漏洞"
    }

    Write-Step "检查过时依赖..."
    $outdated = dotnet list package --outdated 2>&1
    if ($outdated -match "Project") {
        Write-WarningMsg "存在过时依赖包"
        if ($Verbose) { $outdated | Write-Host }
    }
    else {
        Write-Success "所有依赖为最新"
    }
}
else {
    Write-Info "已跳过安全检查"
}

# 打包
if (-not $SkipPack -and $passed) {
    Write-Step "打包 NuGet 包..."
    $projects = @(
        "FlexArch.OutBox.Abstractions",
        "FlexArch.OutBox.Core",
        "FlexArch.OutBox.Persistence.EFCore",
        "FlexArch.OutBox.Publisher.RabbitMQ"
    )

    foreach ($proj in $projects) {
        $csproj = "$proj/$proj.csproj"
        if (Test-Path $csproj) {
            dotnet pack $csproj --no-build -c Release -o ./packages
            if ($LASTEXITCODE -ne 0) {
                Write-Failure "打包失败: $proj"
                $passed = $false
            }
            else {
                Write-Success "已打包: $proj"
            }
        }
        else {
            Write-WarningMsg "未找到项目: $csproj"
        }
    }
}
else {
    Write-Info "跳过 NuGet 打包"
}

# 总结
$duration = (Get-Date) - $startTime
Write-Host "`n📊 验证总结：" -ForegroundColor Magenta
if ($passed) {
    Write-Info "验证时间: $duration"
    Write-Success "所有检查通过 🎉"
    exit 0
}
else {
    Write-Failure "部分检查失败，请修复后重试"
    exit 1
}