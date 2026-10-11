# CrossApp — предметна область «Склад»

## Лабораторна 4: доменна модель

Сутність `Core.Domain.Product` (товар на складі) — приватні поля, властивості лише для читання,
приватний конструктор, єдина точка створення — `Product.Create`.
`ProductDto` (тиждень 3) лишився форматом даних; зв'язок — `ToDto()` / `Product.FromDto()`.

### Інваріанти

1. `Id`, `SKU`, назва й одиниця виміру не порожні — `ArgumentException` (`Create`).
2. Початковий залишок не від'ємний — `ArgumentOutOfRangeException` (`Create`).
3. Кількість приходу та видачі більша за нуль — `ArgumentOutOfRangeException` (`RegisterArrival`, `Issue`).
4. Не можна видати більше, ніж є на складі — `InvalidOperationException` (`Issue`); стан не змінюється.
5. `FromDto` проходить ті самі перевірки, що й `Create`.

Domain не залежить від Console, File і Cli.

### Запуск

    dotnet run --project src/Cli
    dotnet run --project src/Cli -- data/sample_domain.csv

### Додаткові завдання

- `DomainImportResult` — сутності + перелік рядків, що не пройшли інваріанти.
- `IssuingService` — правила на кількох сутностях: не більше 5 відкритих документів на отримувача; видача перевіряє залишки всіх рядків до списання.
- `DocumentStatus` + `DocumentStatusRules` — стани Draft/Confirmed/Cancelled і переходи через switch expression.
