using ElectroShop.Core.Models.Coupons;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Views;

internal static class CouponViews
{
    public static Table Table(IReadOnlyList<Coupon> coupons, DateTime now, bool adminMode)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand();
        table.AddColumn("[bold]Код[/]");
        table.AddColumn("[bold]Тип[/]");
        table.AddColumn("[bold]Знижка[/]");
        table.AddColumn(new TableColumn("[bold]Мін. сума[/]").RightAligned());
        table.AddColumn("[bold]Діє[/]");
        if (adminMode)
        {
            table.AddColumn(new TableColumn("[bold]Використано[/]").RightAligned());
            table.AddColumn("[bold]Стан[/]");
        }

        foreach (var c in coupons)
        {
            var row = new List<string>
            {
                $"[bold mediumpurple1]{c.Code}[/]",
                c.TypeName,
                Markup.Escape(c.ValueText),
                c.MinOrderAmount > 0 ? Theme.MoneyShort(c.MinOrderAmount) : "—",
                Period(c)
            };
            if (adminMode)
            {
                row.Add(c.UsageLimit is { } limit ? $"{c.UsedCount} / {limit}" : $"{c.UsedCount} / ∞");
                row.Add(State(c, now));
            }
            table.AddRow(row.ToArray());
        }

        if (coupons.Count == 0)
            table.AddRow($"[{Theme.MutedTag}]Купонів немає[/]");
        return table;
    }

    public static string Label(Coupon c) => $"[mediumpurple1]{c.Code}[/] [{Theme.MutedTag}]· {Markup.Escape(c.Description)}[/]";

    private static string Period(Coupon c) => c.ValidUntil is { } until
        ? $"{c.ValidFrom:dd.MM.yy} – {until:dd.MM.yy}"
        : $"з {c.ValidFrom:dd.MM.yy}, безстроково";

    private static string State(Coupon c, DateTime now) => c switch
    {
        { IsActive: false } => "[grey50]деактивовано[/]",
        _ when c.IsExpired(now) => "[red1]прострочено[/]",
        _ when c.ValidFrom.Date > now.Date => "[deepskyblue1]ще не діє[/]",
        { IsUsageLimitReached: true } => "[orange1]вичерпано[/]",
        _ => "[springgreen2]активний[/]"
    };
}
