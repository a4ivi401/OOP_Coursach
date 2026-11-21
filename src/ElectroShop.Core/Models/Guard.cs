using System.Text.RegularExpressions;

namespace ElectroShop.Core.Models;

/// <summary>Спільні перевірки для setter-ів моделей.</summary>
internal static class Guard
{
    public static string Text(string? value, string field, int min, int max)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0)
            throw new ArgumentException($"{field} не може бути порожнім.");
        if (text.Length < min || text.Length > max)
            throw new ArgumentException($"{field}: довжина має бути від {min} до {max} символів.");
        return text;
    }

    public static string OptionalText(string? value, string field, int max)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length > max)
            throw new ArgumentException($"{field}: не більше {max} символів.");
        return text;
    }

    public static string Pattern(string? value, string field, Regex regex, string hint)
    {
        var text = value?.Trim() ?? string.Empty;
        if (!regex.IsMatch(text))
            throw new ArgumentException($"{field} має некоректний формат. {hint}");
        return text;
    }

    public static T Range<T>(T value, string field, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(field, value, $"{field} має бути в межах від {min} до {max}.");
        return value;
    }

    public static Guid NotEmpty(Guid value, string field)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{field} не вказано.");
        return value;
    }
}
