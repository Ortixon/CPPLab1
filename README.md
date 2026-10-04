# CrossApp

Лабораторна робота 3 (предметна область — «Склад»): record-типи, pattern matching, імпорт CSV/JSON.

## Структура

```
data/sample.csv (з дефектами), sample_ok.csv, sample_noheader.csv, sample.json, mixed.csv
src/Core/Dto      ProductDto, WarehouseDto, ImportResult<T>, ImportStats, MixedImportResult
src/Core/Import   ProductCsvImporter, ProductJsonImporter, MixedCsvImporter
src/Cli/Program.cs
```

## Формат data/sample.csv

* роздільник — крапка з комою `;`
* кодування — UTF-8
* колонки: `id;sku;name;unit;quantity`; перший рядок із заголовком необов'язковий
* порожні рядки та рядки, що починаються з `#`, ігноруються
* числа розбираються через `CultureInfo.InvariantCulture`

Формат `data/mixed.csv`: `P;id;sku;name;unit;quantity` — товар, `W;id;name[;address]` — склад.
Формат `data/sample.json`: масив об'єктів із полями `id, sku, name, unit, quantity, note`.

## Запуск

```
dotnet run --project src/Cli                       # data/sample.csv
dotnet run --project src/Cli -- data/sample.json   # імпортер за розширенням
dotnet run --project src/Cli -- --mixed            # data/mixed.csv (рядки P/W)
dotnet run --project src/Cli -- data/nope.csv      # файл не знайдено, код виходу 1
```
