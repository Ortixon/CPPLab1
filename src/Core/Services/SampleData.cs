using Core.Domain;

namespace Core.Services;

public static class SampleData
{
    public static IEnumerable<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 120),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 18),
        Product.Create("P-003", "SKU-003", "Саморіз 3.5x35", "кг", 7),
        Product.Create("P-004", "SKU-004", "Фарба водоемульсійна", "шт", 36),
        Product.Create("P-005", "SKU-005", "Герметик силіконовий", "шт", 4),
        Product.Create("P-006", "SKU-006", "Профіль CD-60", "м", 800),
        Product.Create("P-007", "SKU-007", "Кабель ВВГ 3x2.5", "м", 1500),
        Product.Create("P-008", "SKU-008", "Двері міжкімнатні", "шт", 12),
    ];
}
