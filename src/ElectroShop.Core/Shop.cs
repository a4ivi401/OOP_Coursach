using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Persistence;
using ElectroShop.Core.Pricing;
using ElectroShop.Core.Services;

namespace ElectroShop.Core;

/// <summary>Фасад бізнес-логіки: єдина точка входу для інтерфейсу користувача.</summary>
public sealed class Shop
{
    private readonly IDataStorage<ShopSnapshot> _storage;
    private readonly ShopContext _context = new();
    private readonly TimeProvider _time;

    public Shop(IDataStorage<ShopSnapshot> storage, TimeProvider? time = null)
    {
        _storage = storage;
        _time = time ?? TimeProvider.System;

        Catalog = new CatalogService(_context);
        Customers = new CustomerService(_context, _time);
        Carts = new CartService(_context);
        Coupons = new CouponService(_context, _time);
        Orders = new OrderService(_context, Carts, Coupons, new PriceCalculator(), _time);
        Reports = new ReportService(_context);
        Admin = new AdminAccess();
    }

    public CatalogService Catalog { get; }
    public CustomerService Customers { get; }
    public CartService Carts { get; }
    public CouponService Coupons { get; }
    public OrderService Orders { get; }
    public ReportService Reports { get; }
    public AdminAccess Admin { get; }

    public string StorageLocation => _storage.Location;

    public bool HasSavedData => _storage.Exists;

    public bool IsEmpty => _context.IsEmpty;

    public DateTime? LastSavedAt { get; private set; }

    public void Load()
    {
        var snapshot = _storage.Load();
        _context.LoadFrom(snapshot);
        LastSavedAt = snapshot.SavedAt;
    }

    public void Save()
    {
        var now = _time.GetLocalNow().DateTime;
        _storage.Save(_context.ToSnapshot(now));
        LastSavedAt = now;
    }

    public string BackupStorage() => _storage.Backup();

    public void Reset() => _context.Clear();

    public void SeedDemoData()
    {
        _context.Clear();
        DemoDataSeeder.Seed(_context, _time);
    }
}
