using FlexArch.OutBox.EFCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FlexArch.OutBox.Persistence.EFCore;

/// <summary>
/// ModelBuilder扩展方法，用于方便地应用OutBox实体配置
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// 应用所有OutBox相关实体配置，包括实体映射和优化索引
    /// </summary>
    /// <param name="modelBuilder">EF Core模型构建器</param>
    /// <returns>模型构建器，支持链式调用</returns>
    public static ModelBuilder ApplyOutboxEntityConfigurations(this ModelBuilder modelBuilder)
    {
        // 应用OutboxMessage配置
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // 应用DeadLetterMessage配置
        modelBuilder.ApplyConfiguration(new DeadLetterMessageConfiguration());

        return modelBuilder;
    }
}
