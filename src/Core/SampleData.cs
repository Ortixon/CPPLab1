using Core.Domain;
using Core.Services;

namespace Core;

/// <summary>18 записів, 4 одиниці виміру; 3 позиції з залишком &lt; 10; є однакові залишки (120) і "ШТ" у іншому регістрі.</summary>
public static class SampleData
{
    public static readonly (string Sku, string Name, string Unit, int Quantity)[] Items =
    [
        ("SKU-101", "Кабель ВВГ 3x2.5",        "м",  75),
        ("SKU-102", "Провід ПВС 2x1.5",        "м",  120),
        ("SKU-103", "Кабель-канал 25x16",      "м",  60),
        ("SKU-104", "Шланг поливальний",       "м",  45),
        ("SKU-201", "Саморіз 3.5x35",          "кг", 7),
        ("SKU-202", "Цвяхи 80 мм",             "кг", 32),
        ("SKU-203", "Гвинт М6x30",             "кг", 18),
        ("SKU-301", "Герметик силіконовий",    "шт", 4),
        ("SKU-302", "Скоба 10 мм",             "шт", 85),
        ("SKU-303", "Дюбель 6x40",             "шт", 120),
        ("SKU-304", "Лампа LED E27",           "шт", 64),
        ("SKU-305", "Вимикач одноклавішний",   "шт", 9),
        ("SKU-306", "Розетка із заземленням",  "шт", 40),
        ("SKU-307", "Автомат 16А",             "шт", 28),
        ("SKU-308", "Стрічка ізоляційна",      "ШТ", 55),
        ("SKU-401", "Фарба біла",              "л",  22),
        ("SKU-402", "Розчинник",               "л",  12),
        ("SKU-403", "Олія мастильна",          "л",  15),
    ];

    public static List<Product> Products() => Items
        .Select((x, i) => Product.Create($"P-{i + 1:D3}", x.Sku, x.Name, x.Unit, x.Quantity))
        .ToList();

    public static void Load(CatalogService service)
    {
        foreach (var (sku, name, unit, qty) in Items)
            service.Add(sku, name, unit, qty);
    }
}