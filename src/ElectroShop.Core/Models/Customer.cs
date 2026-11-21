using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models;

public partial class Customer : EntityBase
{
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _address = string.Empty;
    private decimal _totalSpent;

    public Customer() { }

    public Customer(string fullName, string email, string phone, string address)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
    }

    [GeneratedRegex(@"^[\p{L}][\p{L}'’ʼ\- ]+$")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"^[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,}$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^\+380\d{9}$")]
    private static partial Regex PhoneRegex();

    public string FullName
    {
        get => _fullName;
        set
        {
            var name = Guard.Text(value, "ПІБ", 3, 60);
            _fullName = Guard.Pattern(name, "ПІБ", NameRegex(), "Лише літери, пробіл, дефіс та апостроф.");
        }
    }

    public string Email
    {
        get => _email;
        set => _email = Guard.Pattern(value?.ToLowerInvariant(), "Email", EmailRegex(), "Приклад: name@mail.com.");
    }

    public string Phone
    {
        get => _phone;
        set => _phone = Guard.Pattern(NormalizePhone(value), "Телефон", PhoneRegex(), "Формат: +380XXXXXXXXX.");
    }

    public string Address
    {
        get => _address;
        set => _address = Guard.Text(value, "Адреса", 5, 150);
    }

    public DateTime RegisteredAt { get; init; } = DateTime.Now;

    public decimal TotalSpent
    {
        get => _totalSpent;
        set => _totalSpent = Guard.Range(value, "Сума покупок", 0m, decimal.MaxValue);
    }

    public Cart Cart { get; init; } = new();

    [JsonIgnore]
    public LoyaltyLevel LoyaltyLevel => LoyaltyProgram.GetLevel(TotalSpent);

    [JsonIgnore]
    public override string DisplayName => $"{FullName} <{Email}>";

    // Дозволяє вводити телефон з пробілами, дужками чи без +38
    public static string NormalizePhone(string? raw)
    {
        var digits = new string((raw ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && digits.StartsWith('0'))
            digits = "38" + digits;
        return "+" + digits;
    }
}
