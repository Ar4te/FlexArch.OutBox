#!/bin/bash

set -e
start=$(date +%s)
echo -e "\n🚀 FlexArch.OutBox 验证开始\n"

SKIP_TESTS=false
SKIP_FORMAT=false
SKIP_SECURITY=false
SKIP_PACK=false
VERBOSE=false

while [[ "$#" -gt 0 ]]; do
    case $1 in
        --skip-tests) SKIP_TESTS=true ;;
        --skip-format) SKIP_FORMAT=true ;;
        --skip-security) SKIP_SECURITY=true ;;
        --skip-pack) SKIP_PACK=true ;;
        --verbose) VERBOSE=true ;;
        -h|--help)
            echo "用法: ./validate-code.sh [选项]"
            echo "  --skip-tests       跳过测试"
            echo "  --skip-format      跳过格式检查"
            echo "  --skip-security    跳过安全检查"
            echo "  --skip-pack        跳过打包"
            echo "  --verbose          显示详细信息"
            exit 0 ;;
    esac
    shift
done

SLN=$(find . -name "*.sln" | head -n 1)
[[ -z "$SLN" ]] && echo "❌ 未找到解决方案文件" && exit 1

dotnet --version | grep -q "^8" && echo "✅ 检测到 .NET 8 SDK" || echo "⚠️ 建议使用 .NET 8 SDK"

echo "🔄 正在恢复依赖..."
dotnet restore "$SLN" --verbosity quiet

if [ "$SKIP_FORMAT" = false ]; then
    echo "🔄 正在进行代码格式检查..."
    dotnet format "$SLN" --verify-no-changes --verbosity minimal || {
        echo "❌ 代码格式不规范，请执行 'dotnet format'"
        exit 1
    }
fi

echo "🔄 正在构建项目..."
dotnet build "$SLN" -c Release --no-restore -p:TreatWarningsAsErrors=true

if [ "$SKIP_TESTS" = false ]; then
    echo "🔄 正在运行测试..."
    dotnet test "$SLN" --no-build -c Release --logger "console;verbosity=detailed"
    echo "🔄 正在生成覆盖率报告..."
    dotnet test --no-build -c Release --collect:"XPlat Code Coverage" --results-directory TestResults
fi

if [ "$SKIP_SECURITY" = false ]; then
    echo "🔄 安全漏洞扫描..."
    vuln=$(dotnet list package --vulnerable --include-transitive)
    echo "$vuln" | grep -q "vulnerable" && echo "❌ 发现漏洞！" && echo "$vuln" && exit 1 || echo "✅ 未发现漏洞"

    echo "🔄 检查过时依赖..."
    dotnet list package --outdated
fi

if [ "$SKIP_PACK" = false ]; then
    echo "🔄 开始打包..."
    mkdir -p packages
    for proj in FlexArch.OutBox.Abstractions FlexArch.OutBox.Core FlexArch.OutBox.Persistence.EFCore FlexArch.OutBox.Publisher.RabbitMQ; do
        if [ -f "$proj/$proj.csproj" ]; then
            dotnet pack "$proj/$proj.csproj" -c Release --no-build -o ./packages
        fi
    done
fi

end=$(date +%s)
echo "✅ 所有检查完成！耗时 $((end - start)) 秒"
