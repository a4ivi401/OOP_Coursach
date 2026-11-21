using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;

namespace ElectroShop.Tests;

public class ModelValidationTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Theory]
    [InlineData("")]
    [InlineData("phone")]
    [InlineData("P-1")]
    [InlineData("PHONE1-12")]
    public void Product_RejectsInvalidSku(string sku) =>
        Assert.Throws<ArgumentException>(() => new Product(sku, "Phone", "Brand", CategoryId, 100m, 1));

    [Fact]
    public void Product_NormalizesSkuToUpperCase()
    {
        var product = new Product("phn-123", "Phone", "Brand", CategoryId, 100m, 1);
        Assert.Equal("PHN-123", product.Sku);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Product_RejectsNonPositivePrice(decimal price) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("PHN-123", "Phone", "Brand", CategoryId, price, 1));

    [Fact]
    public void Product_RejectsNegativeStock() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("PHN-123", "Phone", "Brand", CategoryId, 10m, -1));

    [Fact]
    public void Product_ReserveMoreThanStock_ThrowsOutOfStock()
    {
        var product = new Product("PHN-123", "Phone", "Brand", CategoryId, 10m, 2);

        var ex = Assert.Throws<OutOfStockException>(() => product.ReserveStock(3));
        Assert.Equal(2, ex.Available);
        Assert.Equal(2, product.StockQuantity);
    }

    [Fact]
    public void Product_CompareTo_SortsByDisplayName()
    {
        var a = new Product("AAA-111", "Alpha", "Acme", CategoryId, 10m, 1);
        var b = new Product("BBB-111", "Beta", "Acme", CategoryId, 5m, 1);
        Assert.True(a.CompareTo(b) < 0);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("")]
    public void Customer_RejectsInvalidEmail(string email) =>
        Assert.Throws<ArgumentException>(() => new Customer("Іван Петренко", email, "+380501234567", "Київ, вул. 1"));

    [Theory]
    [InlineData("0501234567", "+380501234567")]
    [InlineData("+38 (050) 123-45-67", "+380501234567")]
    public void Customer_NormalizesPhone(string raw, string expected)
    {
        var customer = new Customer("Іван Петренко", "ivan@mail.com", raw, "Київ, вул. 1");
        Assert.Equal(expected, customer.Phone);
    }

    [Fact]
    public void Customer_RejectsNameWithDigits() =>
        Assert.Throws<ArgumentException>(() => new Customer("R2D2", "r2@mail.com", "+380501234567", "Київ, вул. 1"));

    [Fact]
    public void Customer_AllowsApostropheInName()
    {
        var customer = new Customer("Мар'яна Ковальчук", "m@mail.com", "+380501234567", "Київ, вул. 1");
        Assert.Equal("Мар'яна Ковальчук", customer.FullName);
    }
}
