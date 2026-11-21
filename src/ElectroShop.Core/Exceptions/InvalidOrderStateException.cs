using ElectroShop.Core.Models;

namespace ElectroShop.Core.Exceptions;

public class InvalidOrderStateException : ShopException
{
    public OrderStatus From { get; }
    public OrderStatus To { get; }

    public InvalidOrderStateException(int orderNumber, OrderStatus from, OrderStatus to)
        : base($"Замовлення #{orderNumber:D5}: неможливо змінити статус «{from.ToDisplayName()}» на «{to.ToDisplayName()}».")
    {
        From = from;
        To = to;
    }
}
