namespace ElectroShop.Core.Models;

public record OrderStatusChange(OrderStatus Status, DateTime ChangedAt);
