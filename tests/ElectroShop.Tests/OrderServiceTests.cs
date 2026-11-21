using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Services;

namespace ElectroShop.Tests;

public class OrderServiceTests
{
    private readonly ShopFixture _f = new();

    [Fact]
    public void AddToCart_MoreThanInStock_ThrowsOutOfStock()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Headphones.Id, 2);
        Assert.Throws<OutOfStockException>(() => _f.Shop.Carts.Add(_f.Customer, _f.Headphones.Id, 1));
        Assert.Equal(2, _f.Customer.Cart.QuantityOf(_f.Headphones.Id));
    }

    [Fact]
    public void AddToCart_ProductWithZeroStock_ThrowsOutOfStock() =>
        Assert.Throws<OutOfStockException>(() => _f.Shop.Carts.Add(_f.Customer, _f.Cable.Id, 1));

    [Fact]
    public void PlaceOrder_EmptyCart_Throws() =>
        Assert.Throws<EmptyCartException>(() => _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup));

    [Fact]
    public void PlaceOrder_ReservesStock_ClearsCart_UpdatesSpending()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 2);

        var order = _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Courier);

        Assert.Equal(1, order.Number);
        Assert.Equal(3, _f.Phone.StockQuantity);
        Assert.True(_f.Customer.Cart.IsEmpty);
        Assert.Equal(20_000m, _f.Customer.TotalSpent);
        Assert.Equal(OrderStatus.New, order.Status);
    }

    [Fact]
    public void PlaceOrder_WithInvalidCoupon_ChangesNothing()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);

        Assert.Throws<InvalidDiscountException>(() =>
            _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup, "NOSUCHCODE"));

        Assert.Equal(5, _f.Phone.StockQuantity);
        Assert.False(_f.Customer.Cart.IsEmpty);
    }

    [Fact]
    public void PlaceOrder_WithCoupon_RegistersUsage()
    {
        var coupon = new PercentageCoupon("TEN10", 10) { ValidFrom = _f.Time.Now.AddDays(-1) };
        _f.Shop.Coupons.Add(coupon);
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);

        var order = _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup, "ten10");

        Assert.Equal(1_000m, order.Pricing.CouponDiscount);
        Assert.Equal(1, coupon.UsedCount);
        Assert.Equal("TEN10", order.CouponCode);
    }

    [Fact]
    public void PlaceOrder_RaisesLowStockEvent()
    {
        var raised = new List<Product>();
        _f.Shop.Orders.LowStock += (_, e) => raised.Add(e.Product);
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 3);

        _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup);

        Assert.Contains(_f.Phone, raised);
    }

    [Fact]
    public void Cancel_ReturnsStockAndSpending()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 2);
        var order = _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup);

        _f.Shop.Orders.Cancel(order);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(5, _f.Phone.StockQuantity);
        Assert.Equal(0m, _f.Customer.TotalSpent);
    }

    [Fact]
    public void StatusFlow_FollowsLifecycle()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);
        var order = _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup);

        Assert.Throws<InvalidOrderStateException>(() => _f.Shop.Orders.ChangeStatus(order, OrderStatus.Delivered));

        _f.Shop.Orders.ChangeStatus(order, OrderStatus.Paid);
        _f.Shop.Orders.ChangeStatus(order, OrderStatus.Shipped);
        Assert.Throws<InvalidOrderStateException>(() => _f.Shop.Orders.Cancel(order));
        _f.Shop.Orders.ChangeStatus(order, OrderStatus.Delivered);

        Assert.Equal(4, order.History.Count);
    }

    [Fact]
    public void OrderNumbers_AreSequential()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);
        _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup);
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);
        var second = _f.Shop.Orders.PlaceOrder(_f.Customer, DeliveryMethod.Pickup);

        Assert.Equal(2, second.Number);
        Assert.Same(second, _f.Shop.Orders.GetByNumber(2));
    }
}
