using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // Єдине місце, де піднімається подія. Core лише повідомляє, ЩО сталося.
    private void OnChanged(Product p, CatalogChangeKind kind, int delta, int? quantityAfter = null) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, quantityAfter ?? p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity);   // після зміни, не до
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity);   // якщо цей рядок кине виняток — події не буде
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public void Remove(string id)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        var removedQuantity = product.Quantity;
        _store.Remove(id);
        OnChanged(product, CatalogChangeKind.Removed, -removedQuantity, quantityAfter: 0);
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    // Func: критерій задає той, хто викликає; результат — значення, тому не подія.
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    // Додаткове завдання 2: Action<string> — звіт про пропущені рядки.
    // Формат рядка: SKU;Назва;Одиниця;Кількість. Повертає кількість доданих записів.
    public int Import(IEnumerable<string> lines, Action<string> onSkipped)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(onSkipped);

        int imported = 0, lineNo = 0;
        foreach (var line in lines)
        {
            lineNo++;
            var parts = line.Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length != 4) { onSkipped($"рядок {lineNo}: очікується 4 поля, отримано {parts.Length}"); continue; }
            if (!int.TryParse(parts[3], out var qty)) { onSkipped($"рядок {lineNo}: кількість «{parts[3]}» не є числом"); continue; }
            try { Add(parts[0], parts[1], parts[2], qty); imported++; }
            catch (ArgumentException ex) { onSkipped($"рядок {lineNo}: {ex.Message.Split('(')[0].Trim()}"); }
        }
        return imported;
    }
}
