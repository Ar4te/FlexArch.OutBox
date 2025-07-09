using FlexArch.OutBox.Abstractions.IStores;
using FlexArch.OutBox.Persistence.EFCore.Models;
using FlexArch.OutBox.Persistence.EFCore.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlexArch.OutBox.Persistence.EFCore;

/// <summary>
/// EF Core持久化扩展方法
/// </summary>
public static class PersistenceExtensions
{
    /// <summary>
    /// 添加EF Core持久化支持
    /// </summary>
    /// <typeparam name="TDbContext">DbContext类型</typeparam>
    /// <param name="service">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddEfPersistence<TDbContext>(this IServiceCollection service) where TDbContext : DbContext
    {
        service.AddScoped<IOutboxStore, EfOutboxStore<TDbContext>>();
        service.AddScoped<IExtendedOutboxStore, EfOutboxStore<TDbContext>>();
        service.AddScoped<IDeadLetterStore, EfDeadLetterStore<TDbContext>>();

        // OutboxMessageFactory 是可选的，如果需要可以手动注册：
        // service.AddSingleton<OutboxMessageFactory>();

        return service;
    }



    /// <summary>
    /// 【推荐方式2】应用OutBox推荐的数据库配置（包括索引）到DbContext
    /// 这是向后兼容的别名方法
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder实例</param>
    /// <returns>ModelBuilder实例以支持链式调用</returns>
    public static ModelBuilder UseOutboxConfiguration(this ModelBuilder modelBuilder)
    {
        return modelBuilder.ApplyOutboxEntityConfigurations();
    }

    /// <summary>
    /// 【方式3】自动发现并应用所有OutBox相关配置
    /// 这会自动扫描程序集中的IEntityTypeConfiguration实现
    /// 优点：完全自动化，零侵入
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder实例</param>
    /// <param name="assemblies">要扫描的程序集，为空则扫描当前程序集</param>
    /// <returns>ModelBuilder实例以支持链式调用</returns>
    public static ModelBuilder ApplyOutboxConfigurationsFromAssembly(this ModelBuilder modelBuilder, params System.Reflection.Assembly[] assemblies)
    {
        System.Reflection.Assembly[] assembliesToScan = assemblies?.Length > 0
            ? assemblies
            : new[] { typeof(PersistenceExtensions).Assembly };

        foreach (System.Reflection.Assembly? assembly in assembliesToScan)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly, type =>
                type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                    (i.GetGenericArguments()[0] == typeof(OutboxMessage) ||
                     i.GetGenericArguments()[0] == typeof(DeadLetterMessage))));
        }

        return modelBuilder;
    }

    /// <summary>
    /// 【方式4】手动配置（如果你不想使用IEntityTypeConfiguration）
    /// 直接在ModelBuilder上配置实体映射
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder实例</param>
    /// <returns>ModelBuilder实例以支持链式调用</returns>
    public static ModelBuilder ConfigureOutboxManually(this ModelBuilder modelBuilder)
    {
        ConfigureOutboxMessage(modelBuilder);
        ConfigureDeadLetterMessage(modelBuilder);
        return modelBuilder;
    }

    private static void ConfigureOutboxMessage(ModelBuilder modelBuilder)
    {
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder = modelBuilder.Entity<OutboxMessage>();

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.Type).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.SentAt).IsRequired(false);

        builder.Property(x => x.Headers)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object?>()
            );

        builder.HasIndex(x => new { x.Status, x.CreatedAt }, "IX_OutboxMessage_Status_CreatedAt");
        builder.HasIndex(x => x.SentAt, "IX_OutboxMessage_SentAt");
        builder.HasIndex(x => x.Type, "IX_OutboxMessage_Type");
        builder.HasIndex(x => new { x.Status, x.SentAt }, "IX_OutboxMessage_Status_SentAt");
    }

    private static void ConfigureDeadLetterMessage(ModelBuilder modelBuilder)
    {
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DeadLetterMessage> builder = modelBuilder.Entity<DeadLetterMessage>();

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.Type).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.FailedAt).IsRequired();
        builder.Property(x => x.ErrorReason).HasMaxLength(2000);

        builder.Property(x => x.Headers)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object?>()
            );

        builder.HasIndex(x => x.FailedAt, "IX_DeadLetterMessage_FailedAt");
        builder.HasIndex(x => x.Type, "IX_DeadLetterMessage_Type");
        builder.HasIndex(x => x.CreatedAt, "IX_DeadLetterMessage_CreatedAt");
        builder.HasIndex(x => new { x.Type, x.FailedAt }, "IX_DeadLetterMessage_Type_FailedAt");
    }
}
