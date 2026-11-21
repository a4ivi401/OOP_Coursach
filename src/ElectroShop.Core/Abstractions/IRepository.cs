namespace ElectroShop.Core.Abstractions;

public interface IRepository<T> : IEnumerable<T> where T : class, IEntity
{
    int Count { get; }

    T this[Guid id] { get; }

    void Add(T item);
    void AddRange(IEnumerable<T> items);
    bool Remove(Guid id);
    void Clear();

    T GetById(Guid id);
    T? FindById(Guid id);
    IReadOnlyList<T> GetAll();
    IReadOnlyList<T> Find(Predicate<T> match);
    T? FirstOrDefault(Predicate<T> match);
    bool Exists(Predicate<T> match);
}
