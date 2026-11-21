namespace ElectroShop.Core.Models;

public static class OrderStatusExtensions
{
    public static string ToDisplayName(this OrderStatus status) => status switch
    {
        OrderStatus.New => "Нове",
        OrderStatus.Paid => "Оплачено",
        OrderStatus.Shipped => "Відправлено",
        OrderStatus.Delivered => "Доставлено",
        OrderStatus.Cancelled => "Скасовано",
        _ => status.ToString()
    };

    // Дозволені переходи життєвого циклу замовлення
    public static IReadOnlyList<OrderStatus> NextStatuses(this OrderStatus status) => status switch
    {
        OrderStatus.New => [OrderStatus.Paid, OrderStatus.Cancelled],
        OrderStatus.Paid => [OrderStatus.Shipped, OrderStatus.Cancelled],
        OrderStatus.Shipped => [OrderStatus.Delivered],
        _ => []
    };

    public static bool CanMoveTo(this OrderStatus from, OrderStatus to) => from.NextStatuses().Contains(to);
}
