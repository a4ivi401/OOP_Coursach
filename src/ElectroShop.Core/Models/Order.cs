using System.Text.Json.Serialization;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models;

public class Order : EntityBase, IComparable<Order>
{
    private string _shippingAddress = string.Empty;

    public Order() { }

    public Order(int number, Customer customer, IEnumerable<OrderItem> items, PriceBreakdown pricing,
        DeliveryMethod delivery, string? couponCode, DateTime createdAt)
    {
        Number = number;
        CustomerId = customer.Id;
        ShippingAddress = delivery == DeliveryMethod.Pickup ? "Самовивіз" : customer.Address;
        ItemsData = items.ToList();
        Pricing = pricing;
        Delivery = delivery;
        CouponCode = couponCode;
        CreatedAt = createdAt;
        HistoryData.Add(new OrderStatusChange(OrderStatus.New, createdAt));
    }

    public int Number { get; init; }

    public Guid CustomerId { get; init; }

    public DateTime CreatedAt { get; init; }

    [JsonInclude]
    public OrderStatus Status { get; private set; } = OrderStatus.New;

    public DeliveryMethod Delivery { get; init; }

    public string ShippingAddress
    {
        get => _shippingAddress;
        init => _shippingAddress = Guard.Text(value, "Адреса доставки", 3, 150);
    }

    public string? CouponCode { get; init; }

    public PriceBreakdown Pricing { get; init; } = PriceBreakdown.Empty;

    [JsonInclude, JsonPropertyName("Items")]
    private List<OrderItem> ItemsData { get; set; } = new();

    [JsonInclude, JsonPropertyName("History")]
    private List<OrderStatusChange> HistoryData { get; set; } = new();

    [JsonIgnore]
    public IReadOnlyList<OrderItem> Items => ItemsData;

    [JsonIgnore]
    public IReadOnlyList<OrderStatusChange> History => HistoryData;

    [JsonIgnore]
    public decimal Total => Pricing.Total;

    [JsonIgnore]
    public bool IsActive => Status != OrderStatus.Cancelled;

    [JsonIgnore]
    public bool CanBeCancelled => Status.CanMoveTo(OrderStatus.Cancelled);

    [JsonIgnore]
    public override string DisplayName => $"#{Number:D5}";

    public void ChangeStatus(OrderStatus newStatus, DateTime changedAt)
    {
        if (!Status.CanMoveTo(newStatus))
            throw new InvalidOrderStateException(Number, Status, newStatus);

        Status = newStatus;
        HistoryData.Add(new OrderStatusChange(newStatus, changedAt));
    }

    // Новіші замовлення — першими
    public int CompareTo(Order? other)
    {
        if (other is null) return -1;
        return other.CreatedAt.CompareTo(CreatedAt);
    }
}
