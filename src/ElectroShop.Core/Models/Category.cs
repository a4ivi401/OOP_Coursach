using System.Text.Json.Serialization;
using ElectroShop.Core.Abstractions;

namespace ElectroShop.Core.Models;

public class Category : EntityBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    public Category() { }

    public Category(string name, string description = "")
    {
        Name = name;
        Description = description;
    }

    public string Name
    {
        get => _name;
        set => _name = Guard.Text(value, "Назва категорії", 2, 40);
    }

    public string Description
    {
        get => _description;
        set => _description = Guard.OptionalText(value, "Опис категорії", 200);
    }

    [JsonIgnore]
    public override string DisplayName => Name;
}
