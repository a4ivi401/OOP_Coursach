using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Persistence;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Services;

public sealed class OrderService
{
    private readonly ShopContext _context;
    private readonly CartService _carts;
    private readonly CouponService _coupons;
    private readonly PriceCalculator _calculator;
    private readonly TimeProvider _time;

    public OrderService(ShopContext context, CartService carts, CouponService coupons,
        PriceCalculator calculator, TimeProvider time)
    {
        _context = context;
        _carts = carts;
        _coupons = coupons;
        _calculator = calculator;
        _time = time;
    }

    public event EventHandler<LowStockEventArgs>? LowStock;

    public PriceBreakdown Preview(Customer customer, DeliveryMethod delivery, string? couponCode = null)
    {
        var items = BuildItems(customer);
        var coupon = ResolveCoupon(couponCode, items);
        return _calculator.Calculate(items, delivery, coupon, customer.LoyaltyLevel);
    }

    public Order PlaceOrder(Customer customer, DeliveryMethod delivery, string? couponCode = null)
    {
        // Усі перевірки виконуються до будь-яких змін — замовлення або оформлюється повністю, або ніяк
        var items = BuildItems(customer);
        var coupon = ResolveCoupon(couponCode, items);
        var pricing = _calculator.Calculate(items, delivery, coupon, customer.LoyaltyLevel);
        var now = _time.GetLocalNow().DateTime;

        foreach (var item in items)
        {
            var product = _context.Products.GetById(item.ProductId);
            product.ReserveStock(item.Quantity);
            if (!product.IsAvailable || product.IsLowStock)
                LowStock?.Invoke(this, new LowStockEventArgs(product));
        }

        coupon?.RegisterUsage();
        customer.TotalSpent += GoodsValue(pricing);

        var order = new Order(NextOrderNumber(), customer, items, pricing, delivery, coupon?.Code, now);
        _context.Orders.Add(order);
        customer.Cart.Clear();
        return order;
    }

    public void ChangeStatus(Order order, OrderStatus newStatus)
    {
        if (newStatus == OrderStatus.Cancelled)
            Cancel(order);
        else
            order.ChangeStatus(newStatus, _time.GetLocalNow().DateTime);
    }

    public void Cancel(Order order)
    {
        if (!order.CanBeCancelled)
            throw new InvalidOrderStateException(order.Number, order.Status, OrderStatus.Cancelled);

        order.ChangeStatus(OrderStatus.Cancelled, _time.GetLocalNow().DateTime);

        foreach (var item in order.Items)
            _context.Products.FindById(item.ProductId)?.Restock(item.Quantity);

        if (order.CouponCode is { } code)
            _coupons.FindByCode(code)?.ReleaseUsage();

        if (_context.Customers.FindById(order.CustomerId) is { } customer)
            customer.TotalSpent = Math.Max(0, customer.TotalSpent - GoodsValue(order.Pricing));
    }

    public IReadOnlyList<Order> GetForCustomer(Guid customerId) =>
        Sorted(_context.Orders.Find(o => o.CustomerId == customerId));

    public IReadOnlyList<Order> GetAll(OrderStatus? status = null) =>
        Sorted(_context.Orders.Find(o => status is null || o.Status == status));

    public Order GetByNumber(int number) =>
        _context.Orders.FirstOrDefault(o => o.Number == number)
        ?? throw new EntityNotFoundException("Замовлення", $"#{number:D5}");

    public string GetCustomerName(Order order) =>
        _context.Customers.FindById(order.CustomerId)?.FullName ?? "Видалений клієнт";

    private List<OrderItem> BuildItems(Customer customer)
    {
        var lines = _carts.GetLines(customer);
        if (lines.Count == 0)
            throw new EmptyCartException();

        foreach (var line in lines)
            line.Product.EnsureInStock(line.Quantity);

        return lines.Select(l => new OrderItem(l.Product, l.Quantity)).ToList();
    }

    private Coupon? ResolveCoupon(string? code, IReadOnlyList<OrderItem> items) =>
        string.IsNullOrWhiteSpace(code) ? null : _coupons.Resolve(code, new PricingContext(items));

    private int NextOrderNumber() => _context.Orders.Count == 0 ? 1 : _context.Orders.Max(o => o.Number) + 1;

    private static decimal GoodsValue(PriceBreakdown pricing) => pricing.Total - pricing.DeliveryCost;

    private static List<Order> Sorted(IEnumerable<Order> orders)
    {
        var list = orders.ToList();
        list.Sort();
        return list;
    }
}
