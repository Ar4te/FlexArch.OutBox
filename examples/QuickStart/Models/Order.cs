namespace FlexArch.OutBox.Examples.QuickStart.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    // 外键
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}

// API请求模型
public record CreateOrderRequest(
    string CustomerId,
    List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

// 事件模型
public record OrderCreatedEvent(
    Guid OrderId,
    string CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderItemData> Items
);

public record OrderItemData(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
