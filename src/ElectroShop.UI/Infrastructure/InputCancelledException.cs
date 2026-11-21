namespace ElectroShop.UI.Infrastructure;

/// <summary>Користувач відмовився від введення (порожній рядок або Esc).</summary>
internal sealed class InputCancelledException : Exception
{
    public InputCancelledException() : base("Дію скасовано.") { }
}
