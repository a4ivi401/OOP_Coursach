using ElectroShop.Core.Models;
using Spectre.Console;

namespace ElectroShop.UI.Infrastructure;

internal static class Theme
{
    public static Color Accent { get; } = Color.DeepSkyBlue1;
    public static Color Secondary { get; } = Color.MediumPurple1;
    public static Color Success { get; } = Color.SpringGreen2;
    public static Color Warning { get; } = Color.Orange1;
    public static Color Error { get; } = Color.Red1;
    public static Color Muted { get; } = Color.Grey58;

    public static string AccentTag => "deepskyblue1";
    public static string MutedTag => "grey58";

    public static Style Highlight => new(Color.Black, Accent, Decoration.Bold);

    public static string Money(decimal value) => $"{value:N2} ₴";

    public static string MoneyShort(decimal value) => $"{value:N0} ₴";

    public static string StatusMarkup(OrderStatus status)
    {
        var color = status switch
        {
            OrderStatus.New => "deepskyblue1",
            OrderStatus.Paid => "mediumpurple1",
            OrderStatus.Shipped => "orange1",
            OrderStatus.Delivered => "springgreen2",
            OrderStatus.Cancelled => "grey50 strikethrough",
            _ => "white"
        };
        return $"[{color}]● {status.ToDisplayName()}[/]";
    }

    public static string StockMarkup(Product product) => product switch
    {
        { IsAvailable: false } => "[red1]немає[/]",
        { IsLowStock: true } => $"[orange1]⚠ {product.StockQuantity} шт.[/]",
        _ => $"[springgreen2]{product.StockQuantity} шт.[/]"
    };

    public static string LoyaltyMarkup(LoyaltyLevel level)
    {
        var color = level switch
        {
            LoyaltyLevel.Silver => "silver",
            LoyaltyLevel.Gold => "gold1",
            LoyaltyLevel.Platinum => "aquamarine1",
            _ => "darkorange3"
        };
        return $"[{color}]★ {ElectroShop.Core.Pricing.LoyaltyProgram.ToDisplayName(level)}[/]";
    }
}
