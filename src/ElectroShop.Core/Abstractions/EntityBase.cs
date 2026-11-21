using System.Text.Json.Serialization;

namespace ElectroShop.Core.Abstractions;

public abstract class EntityBase : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonIgnore]
    public abstract string DisplayName { get; }

    public override string ToString() => DisplayName;
}
