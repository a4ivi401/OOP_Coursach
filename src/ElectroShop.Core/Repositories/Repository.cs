using System.Collections;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Exceptions;

namespace ElectroShop.Core.Repositories;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();
    private readonly string _entityName;

    public Repository(string entityName)
    {
        _entityName = entityName;
    }

    public int Count => _items.Count;

    public T this[Guid id] => GetById(id);

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Exists(x => x.Id == item.Id))
            throw new DuplicateEntityException(_entityName, item.Id);
        _items.Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public bool Remove(Guid id) => _items.RemoveAll(x => x.Id == id) > 0;

    public void Clear() => _items.Clear();

    public T GetById(Guid id) => FindById(id) ?? throw new EntityNotFoundException(_entityName, id);

    public T? FindById(Guid id) => _items.Find(x => x.Id == id);

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public IReadOnlyList<T> Find(Predicate<T> match) => _items.FindAll(match);

    public T? FirstOrDefault(Predicate<T> match) => _items.Find(match);

    public bool Exists(Predicate<T> match) => _items.Exists(match);

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
