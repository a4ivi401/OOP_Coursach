namespace ElectroShop.UI.Infrastructure;

internal sealed record MenuItem(string Label, Action Action)
{
    public bool ClosesScreen { get; init; }

    public static MenuItem Back(string label) => new(label, () => { }) { ClosesScreen = true };
}
