using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Додаткове завдання 1: декоратор. Той самий контракт ICatalogStore, нова поведінка —
// кешування читання. Будь-яка зміна (Add/Update/Remove) скидає кеш.
public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{
    private readonly ICatalogStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Product?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Product>? _list;

    public int InnerReads { get; private set; }
    public int CacheHits { get; private set; }

    public IReadOnlyList<Product> List()
    {
        if (_list is not null) { CacheHits++; return _list; }
        InnerReads++;
        return _list = _inner.List();
    }

    public Product? GetById(string id)
    {
        if (_byId.TryGetValue(id, out var cached)) { CacheHits++; return cached; }
        InnerReads++;
        var product = _inner.GetById(id);
        _byId[id] = product;
        return product;
    }

    public void Add(Product item) { _inner.Add(item); Invalidate(); }

    public void Update(Product item) { _inner.Update(item); Invalidate(); }

    public bool Remove(string id)
    {
        var removed = _inner.Remove(id);
        Invalidate();
        return removed;
    }

    private void Invalidate()
    {
        _list = null;
        _byId.Clear();
    }
}
