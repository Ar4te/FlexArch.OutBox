using FlexArch.OutBox.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;

namespace FlexArch.OutBox.Examples.QuickStart.Models;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用OutBox实体配置（包括索引优化）
        modelBuilder.ApplyOutboxEntityConfigurations();

        // 配置Order实体
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();

            // 配置关系
            entity.HasMany(e => e.Items)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 索引
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // 配置OrderItem实体
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Quantity).IsRequired();

            // 索引
            entity.HasIndex(e => e.ProductId);
        });
    }
}
