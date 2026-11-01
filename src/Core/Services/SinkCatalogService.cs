using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

// Додаткове завдання 1: той самий сценарій без події — приймач сповіщень приходить через конструктор.
public sealed class SinkCatalogService(ICatalogStore store, INotificationSink sink)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly INotificationSink _sink = sink ?? throw new ArgumentNullException(nameof(sink));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        _sink.Notify(new CatalogChangedEventArgs(product.Id, CatalogChangeKind.Added, quantity, product.Quantity));
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity);
        _store.Update(product);
        _sink.Notify(new CatalogChangedEventArgs(product.Id, CatalogChangeKind.Received, quantity, product.Quantity));
    }
}
