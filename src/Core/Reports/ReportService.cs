using Core.Domain;

namespace Core.Reports;

/// <summary>
/// Чисті функції звітів: приймають IEnumerable&lt;Product&gt;, повертають DTO.
/// Нічого не знають про Console і про сховище.
/// </summary>
public static class ReportService
{
    // 1) фільтр + сортування + проєкція. Where стоїть ДО OrderBy: сортуємо лише потрібне.
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) топ-N зі стабільним порядком: власний TopBy (другий ключ — унікальний Sku)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.TopBy(p => p.Quantity, p => p.Sku, n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) групування з підрахунком і сумою (шт і ШТ — одна група)
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ThenBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
              .ToList();

    // 4) агрегати з обробкою порожньої послідовності (Average і Max на порожній кидають виняток)
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
               list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // Додатково 3: той самий звіт ByUnit у синтаксисі запитів
    public static IReadOnlyList<GroupSummary> ByUnitQuery(IEnumerable<Product> source) =>
        (from product in source
         group product by product.Unit into g
         orderby g.Sum(p => p.Quantity) descending, g.Key
         select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
        .ToList();

    // Додатково 1: Join — журнал переміщень (переміщення без відомого товару відкидаються)
    public static IReadOnlyList<MovementLine> MovementLog(IEnumerable<Product> products, IEnumerable<Movement> movements) =>
        products.Join(movements,
                      product => product.Id,
                      movement => movement.ProductId,
                      (product, movement) => new MovementLine(product.Sku, product.Name, movement.Delta, movement.Date))
                .OrderBy(l => l.Date).ThenBy(l => l.Sku, StringComparer.Ordinal)
                .ToList();

    // Додатково 1: GroupJoin — зведення по КОЖНОМУ товару, навіть якщо переміщень не було
    public static IReadOnlyList<MovementSummary> Activity(IEnumerable<Product> products, IEnumerable<Movement> movements) =>
        products.GroupJoin(movements,
                           product => product.Id,
                           movement => movement.ProductId,
                           (product, moves) => new MovementSummary(product.Sku, product.Name, moves.Count(), moves.Sum(m => m.Delta)))
                .OrderBy(s => s.Sku, StringComparer.Ordinal)
                .ToList();

    // Додатково 2: TopBy у другому звіті — найактивніші позиції за кількістю переміщень
    public static IReadOnlyList<MovementSummary> MostActive(IEnumerable<Product> products, IEnumerable<Movement> movements, int n) =>
        Activity(products, movements)
            .TopBy(s => s.Moves, s => s.Sku, n)
            .ToList();
}
