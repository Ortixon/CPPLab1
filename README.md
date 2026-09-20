# CrossApp

Наскрізний проєкт з крос-платформного програмування.
Предметна область: Cклад. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).
Призначення: облік залишків товарів по партіях.

## Запуск

dotnet build
dotnet run --project src/Cli

| RID     | Режим               | Розмір publish | Потрібен runtime |
| ------- | ------------------- | -------------- | ---------------- |
| win-x64 | self-contained      | ≈ 76,68 МБ     | ні               |
| win-x64 | framework-dependent | ≈ 0,19 МБ      | так (.NET 10)    |

## Середовище

.NET SDK 10.0, Windows 11 x64
