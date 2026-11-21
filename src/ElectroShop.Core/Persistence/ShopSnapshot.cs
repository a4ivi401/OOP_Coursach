using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;

namespace ElectroShop.Core.Persistence;

/// <summary>Усі дані магазину в одному об'єкті — саме він серіалізується у файл.</summary>
public sealed class ShopSnapshot
{
    public static int CurrentVersion => 1;

    public int Version { get; set; } = CurrentVersion;
    public DateTime SavedAt { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<Coupon> Coupons { get; set; } = new();
}
