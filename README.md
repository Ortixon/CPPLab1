# CrossApp — склад (лабораторна 5)

Сервісний шар: інтерфейси, два сховища, ручний DI.

- Інтерфейс сховища: `ICatalogStore` (`List`, `GetById`, `Add`, `Update`, `Remove`)
- Реалізації: `InMemoryCatalogStore`, `FileCatalogStore`
- Сервіс: `CatalogService` (`Add`, `Receive`, `Issue`, `All`, `Find`) — залежить лише від `ICatalogStore`
- Додатково: `CachingCatalogStore` (декоратор), `CatalogService.Search(Func<Product, bool>)`, `StoreFactory.Create(args)` (прапорці `--file`, `--cache`)
- Composition root: `src/Cli/Program.cs` (єдине місце з `new` для сховищ)
- DI-контейнер (`Microsoft.Extensions.DependencyInjection`) не підключено

Запуск:

    dotnet run --project src/Cli            # у пам'яті
    dotnet run --project src/Cli -- --file  # файл data/catalog.json
