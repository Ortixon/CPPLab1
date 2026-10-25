using Core;
using Core.Abstractions;
using Core.Storage;

// Додаткове завдання 3: фабрика замість тернарного оператора в Program.cs.
// Прапорці: --file (файлове сховище), --cache (обгорнути декоратором кешу).
public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        ICatalogStore store = args.Contains("--file")
            ? new FileCatalogStore(Path.Combine(AppContext.BaseDirectory, "data", "catalog.json"))
            : new InMemoryCatalogStore(SampleData.Products());

        return args.Contains("--cache") ? new CachingCatalogStore(store) : store;
    }
}
