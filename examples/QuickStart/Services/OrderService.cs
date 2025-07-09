using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Examples.QuickStart.Models;
using FlexArch.OutBox.Persistence.EFCore;
using FlexArch.OutBox.Persistence.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlexArch.OutBox.Examples.QuickStart.Services;

public class OrderService
{
    private readonly OrderDbContext _context;
    private readonly IExtendedOutboxStore _outboxStore;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrderDbContext context,
        IExtendedOutboxStore outboxStore,
        ILogger<OrderService> logger)
    {
        _context = context;
        _outboxStore = outboxStore;
        _logger = logger;
    }

    /// <summary>
    /// 创建订单 - 演示OutBox模式确保事务一致性
    /// </summary>
    public async Task<Guid> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("开始创建订单，客户ID: {CustomerId}", request.CustomerId);

        // 使用数据库事务确保数据一致性
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. 创建并保存订单实体
            Order order = CreateOrderFromRequest(request);
            _context.Orders.Add(order);

            // 2. 创建OutBox消息 - 关键步骤！
            var orderCreatedEvent = new OrderCreatedEvent(
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                order.CreatedAt,
                order.Items.Select(item => new OrderItemData(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice
                )).ToList()
            );

            var outboxMessage = new OutboxMessage
            {
                Type = "OrderCreated",
                Payload = JsonSerializer.Serialize(orderCreatedEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Headers = new Dictionary<string, object?>
                {
                    ["CorrelationId"] = Guid.NewGuid().ToString(),
                    ["Source"] = "OrderService",
                    ["Version"] = "1.0",
                    // 演示延迟消息（可选）
                    // ["DelayUntil"] = DateTime.UtcNow.AddSeconds(10).ToString("O")
                }
            };

            // 3. 保存OutBox消息
            await _outboxStore.SaveAsync(outboxMessage);

            // 4. 原子提交 - 确保订单和消息要么都成功，要么都失败
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "订单创建成功！订单ID: {OrderId}, OutBox消息ID: {MessageId}",
                order.Id,
                outboxMessage.Id);

            return order.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "创建订单失败，已回滚事务");
            throw;
        }
    }

    /// <summary>
    /// 获取订单详情
    /// </summary>
    public async Task<Order?> GetOrderAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    /// <summary>
    /// 获取客户的所有订单
    /// </summary>
    public async Task<List<Order>> GetCustomerOrdersAsync(string customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 演示批量创建订单 - 展示OutBox的批量处理能力
    /// </summary>
    public async Task<List<Guid>> CreateBatchOrdersAsync(List<CreateOrderRequest> requests)
    {
        _logger.LogInformation("开始批量创建 {Count} 个订单", requests.Count);

        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var orderIds = new List<Guid>();
            var outboxMessages = new List<OutboxMessage>();

            foreach (CreateOrderRequest request in requests)
            {
                // 创建订单
                Order order = CreateOrderFromRequest(request);
                _context.Orders.Add(order);
                orderIds.Add(order.Id);

                // 创建对应的OutBox消息
                var orderCreatedEvent = new OrderCreatedEvent(
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    order.CreatedAt,
                    order.Items.Select(item => new OrderItemData(
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice
                    )).ToList()
                );

                var outboxMessage = new OutboxMessage
                {
                    Type = "OrderCreated",
                    Payload = JsonSerializer.Serialize(orderCreatedEvent),
                    Headers = new Dictionary<string, object?>
                    {
                        ["CorrelationId"] = Guid.NewGuid().ToString(),
                        ["Source"] = "OrderService.Batch",
                        ["BatchId"] = Guid.NewGuid().ToString()
                    }
                };

                outboxMessages.Add(outboxMessage);
            }

            // 批量保存OutBox消息
            foreach (OutboxMessage message in outboxMessages)
            {
                await _outboxStore.SaveAsync(message);
            }

            // 原子提交
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("批量创建订单成功，共创建 {Count} 个订单", orderIds.Count);
            return orderIds;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "批量创建订单失败，已回滚事务");
            throw;
        }
    }

    private Order CreateOrderFromRequest(CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId
        };

        foreach (CreateOrderItemRequest itemRequest in request.Items)
        {
            var orderItem = new OrderItem
            {
                ProductId = itemRequest.ProductId,
                ProductName = itemRequest.ProductName,
                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                OrderId = order.Id
            };
            order.Items.Add(orderItem);
        }

        // 计算总金额
        order.TotalAmount = order.Items.Sum(item => item.TotalPrice);

        return order;
    }
}
