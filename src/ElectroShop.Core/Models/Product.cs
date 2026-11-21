using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Exceptions;

namespace ElectroShop.Core.Models;

public partial class Product : EntityBase, IComparable<Product>
{
    public static int LowStockThreshold => 3;
    public static decimal MaxPrice => 1_000_000m;

    private string _sku = string.Empty;
    private string _name = string.Empty;
    private string _brand = string.Empty;
    private string _description = string.Empty;
    private Guid _categoryId;
    private decimal _price;
    private int _stock;
    private int _warrantyMonths;

    public Product() { }

    public Product(string sku, string name, string brand, Guid categoryId, decimal price, int stock, int warrantyMonths = 12)
    {
        Sku = sku;
        Name = name;
        Brand = brand;
        CategoryId = categoryId;
        Price = price;
        StockQuantity = stock;
        WarrantyMonths = warrantyMonths;
    }

    [GeneratedRegex(@"^[A-Z]{2,5}-\d{3,6}$")]
    private static partial Regex SkuRegex();

    public string Sku
    {
        get => _sku;
        set => _sku = Guard.Pattern(value?.ToUpperInvariant(), "Артикул", SkuRegex(), "Приклад: PHN-1001.");
    }

    public string Name
    {
        get => _name;
        set => _name = Guard.Text(value, "Назва товару", 2, 80);
    }

    public string Brand
    {
        get => _brand;
        set => _brand = Guard.Text(value, "Бренд", 1, 40);
    }

    public Guid CategoryId
    {
        get => _categoryId;
        set => _categoryId = Guard.NotEmpty(value, "Категорію");
    }

    public decimal Price
    {
        get => _price;
        set => _price = Guard.Range(decimal.Round(value, 2), "Ціна", 0.01m, MaxPrice);
    }

    public int StockQuantity
    {
        get => _stock;
        set => _stock = Guard.Range(value, "Кількість на складі", 0, 100_000);
    }

    public int WarrantyMonths
    {
        get => _warrantyMonths;
        set => _warrantyMonths = Guard.Range(value, "Гарантія (міс.)", 0, 60);
    }

    public string Description
    {
        get => _description;
        set => _description = Guard.OptionalText(value, "Опис", 500);
    }

    // Характеристики зберігаються в словнику, але назовні доступні лише для читання
    [JsonInclude, JsonPropertyName("Specifications")]
    private Dictionary<string, string> SpecificationsData { get; set; } = new();

    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Specifications => SpecificationsData;

    [JsonIgnore]
    public bool IsAvailable => StockQuantity > 0;

    [JsonIgnore]
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    [JsonIgnore]
    public override string DisplayName => $"{Brand} {Name}";

    public void SetSpecification(string key, string value)
    {
        var k = Guard.Text(key, "Назва характеристики", 1, 30);
        SpecificationsData[k] = Guard.Text(value, "Значення характеристики", 1, 60);
    }

    public bool RemoveSpecification(string key) => SpecificationsData.Remove(key);

    public void EnsureInStock(int quantity)
    {
        if (quantity > StockQuantity)
            throw new OutOfStockException(DisplayName, quantity, StockQuantity);
    }

    public void ReserveStock(int quantity)
    {
        Guard.Range(quantity, "Кількість", 1, int.MaxValue);
        EnsureInStock(quantity);
        StockQuantity -= quantity;
    }

    public void Restock(int quantity)
    {
        Guard.Range(quantity, "Кількість поповнення", 1, 10_000);
        StockQuantity += quantity;
    }

    public int CompareTo(Product? other)
    {
        if (other is null) return 1;
        return string.Compare(DisplayName, other.DisplayName, StringComparison.CurrentCultureIgnoreCase);
    }
}
