# CrossApp — лабораторна робота 6: доменні події, Action/Func, лямбди

## Що додано на тижні 6
- `src/Core/Events/CatalogChangeKind.cs` — enum виду зміни (Added, Received, Issued, Removed).
- `src/Core/Events/CatalogChangedEventArgs.cs` — дані події (Id, Kind, Delta, QuantityAfter, At).
- `src/Core/Services/CatalogService.cs` — `event EventHandler<CatalogChangedEventArgs>? Changed`,
  єдиний приватний `OnChanged`, `Issue`, `Remove`, `FindAll(Func<Product,bool>)`, `Import(..., Action<string>)`.
- `src/Cli/Program.cs` — підписка локальним методом + лямбда-лічильник, `-=`, сценарії відмов.
- Додатково: `INotificationSink` (Core), `SinkCatalogService` (Core), `ConsoleSink`/`FileSink`/`CompositeSink` (Cli).

## Правила
- Core не містить `Console` і не пише у файл-журнал; він лише піднімає подію.
- Подія піднімається **після** успішної зміни стану; якщо інваріант кинув виняток — події немає.
- Журнал: `logs/app.log` у каталозі збірки (`AppContext.BaseDirectory`), створюється кодом.

## Запуск
```
dotnet run --project src/Cli            # сховище в пам'яті
dotnet run --project src/Cli -- --file  # файлове сховище (data/catalog.json)
```
