using ElectroShop.Core;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Tests;

public sealed class PersistenceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "electroshop-tests-" + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_dir, "shop.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void DemoData_SurvivesJsonRoundTrip()
    {
        var original = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        original.SeedDemoData();
        original.Save();

        var restored = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        restored.Load();

        Assert.Equal(original.Catalog.GetAllProducts().Count, restored.Catalog.GetAllProducts().Count);
        Assert.Equal(original.Orders.GetAll().Count, restored.Orders.GetAll().Count);
        Assert.Equal(original.Reports.GetSummary(), restored.Reports.GetSummary());

        var specs = restored.Catalog.GetAllProducts().First(p => p.Sku == "PHN-1001").Specifications;
        Assert.Equal("128 ГБ", specs["Пам'ять"]);

        var order = restored.Orders.GetAll()[^1];
        Assert.NotEmpty(order.Items);
        Assert.NotEmpty(order.History);
    }

    [Fact]
    public void Coupons_KeepTheirConcreteTypes()
    {
        var shop = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        shop.SeedDemoData();
        shop.Save();

        var restored = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        restored.Load();

        Assert.IsType<PercentageCoupon>(restored.Coupons.FindByCode("WELCOME10"));
        Assert.IsType<FixedAmountCoupon>(restored.Coupons.FindByCode("MINUS1000"));
        var category = Assert.IsType<CategoryCoupon>(restored.Coupons.FindByCode("AUDIO15"));
        Assert.Equal(15m, category.Percent);
    }

    [Fact]
    public void CustomerCart_IsPersisted()
    {
        var shop = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        shop.SeedDemoData();
        shop.Save();

        var restored = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        restored.Load();

        var andrii = restored.Customers.Login("andrii.shevchuk@ukr.net");
        Assert.False(andrii.Cart.IsEmpty);
    }

    [Fact]
    public void CorruptedFile_ThrowsDataStorageException()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{ this is not json");

        var shop = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        Assert.Throws<DataStorageException>(shop.Load);
    }

    [Fact]
    public void InvalidValueInFile_ThrowsDataStorageException()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, """{ "Categories": [ { "Name": "" } ] }""");

        var shop = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        var ex = Assert.Throws<DataStorageException>(shop.Load);
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    [Fact]
    public void Backup_CreatesCopy()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "broken");

        var backup = new JsonDataStorage<ShopSnapshot>(FilePath).Backup();

        Assert.True(File.Exists(backup));
    }

    [Fact]
    public void DemoData_IsConsistent()
    {
        var shop = new Shop(new JsonDataStorage<ShopSnapshot>(FilePath));
        shop.SeedDemoData();

        var olena = shop.Customers.Login("olena@gmail.com");
        Assert.Equal(LoyaltyLevel.Platinum, olena.LoyaltyLevel);
        Assert.Contains(shop.Orders.GetAll(), o => o.Status == OrderStatus.Cancelled);
    }
}
