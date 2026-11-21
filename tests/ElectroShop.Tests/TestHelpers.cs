using ElectroShop.Core;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Tests;

internal sealed class FakeTime : TimeProvider
{
    public DateTime Now { get; set; } = new(2026, 11, 20, 12, 0, 0);

    public override DateTimeOffset GetUtcNow() => new DateTimeOffset(Now).ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;
}

internal sealed class InMemoryStorage : IDataStorage<ShopSnapshot>
{
    public ShopSnapshot? Saved { get; private set; }

    public string Location => "memory";
    public bool Exists => Saved is not null;
    public void Save(ShopSnapshot data) => Saved = data;
    public ShopSnapshot Load() => Saved ?? throw new InvalidOperationException();
    public string Backup() => "memory.bak";
}

/// <summary>Мінімальний магазин: дві категорії, три товари, один клієнт.</summary>
internal sealed class ShopFixture
{
    public ShopFixture()
    {
        Shop = new Shop(new InMemoryStorage(), Time);
        Phones = Shop.Catalog.AddCategory("Смартфони", "");
        Audio = Shop.Catalog.AddCategory("Аудіо", "");

        Phone = new Product("PHN-0001", "Phone X", "Acme", Phones.Id, 10_000m, 5);
        Headphones = new Product("AUD-0001", "Buds", "Sonic", Audio.Id, 2_000m, 2);
        Cable = new Product("ACC-0001", "Cable", "Acme", Phones.Id, 300m, 0);
        Shop.Catalog.AddProduct(Phone);
        Shop.Catalog.AddProduct(Headphones);
        Shop.Catalog.AddProduct(Cable);

        Customer = Shop.Customers.Register("Тест Тестович", "test@mail.com", "+380501234567", "м. Київ, вул. Тестова, 1");
    }

    public FakeTime Time { get; } = new();
    public Shop Shop { get; }
    public Category Phones { get; }
    public Category Audio { get; }
    public Product Phone { get; }
    public Product Headphones { get; }
    public Product Cable { get; }
    public Customer Customer { get; }
}
