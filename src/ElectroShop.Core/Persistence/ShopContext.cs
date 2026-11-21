using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Repositories;

namespace ElectroShop.Core.Persistence;

/// <summary>Сховище всіх репозиторіїв у пам'яті та перетворення в/із знімка для файлу.</summary>
public sealed class ShopContext
{
    public IRepository<Category> Categories { get; } = new Repository<Category>("Категорію");
    public IRepository<Product> Products { get; } = new Repository<Product>("Товар");
    public IRepository<Customer> Customers { get; } = new Repository<Customer>("Клієнта");
    public IRepository<Order> Orders { get; } = new Repository<Order>("Замовлення");
    public IRepository<Coupon> Coupons { get; } = new Repository<Coupon>("Купон");

    public bool IsEmpty => Categories.Count == 0 && Products.Count == 0 && Customers.Count == 0;

    public ShopSnapshot ToSnapshot(DateTime savedAt) => new()
    {
        SavedAt = savedAt,
        Categories = Categories.ToList(),
        Products = Products.ToList(),
        Customers = Customers.ToList(),
        Orders = Orders.ToList(),
        Coupons = Coupons.ToList()
    };

    public void LoadFrom(ShopSnapshot snapshot)
    {
        Clear();
        Categories.AddRange(snapshot.Categories);
        Products.AddRange(snapshot.Products);
        Customers.AddRange(snapshot.Customers);
        Orders.AddRange(snapshot.Orders);
        Coupons.AddRange(snapshot.Coupons);
    }

    public void Clear()
    {
        Categories.Clear();
        Products.Clear();
        Customers.Clear();
        Orders.Clear();
        Coupons.Clear();
    }
}
