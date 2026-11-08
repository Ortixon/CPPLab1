using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public sealed record BenchLine(string Variant, double Milliseconds, int Result);

/// <summary>Додатково 4: Stopwatch на згенерованих записах, ToList один раз проти багаторазового перебирання.</summary>
public static class ReportBench
{
    public static IReadOnlyList<Product> Generate(int count, int seed = 42)
    {
        var rnd = new Random(seed);
        string[] units = ["шт", "м", "кг", "л"];
        return Enumerable.Range(1, count)
            .Select(i => Product.Create($"g{i:D7}", $"SKU-{i:D6}", $"Товар {i}", units[rnd.Next(units.Length)], rnd.Next(0, 500)))
            .ToList();
    }

    public static IReadOnlyList<BenchLine> Run(int count)
    {
        var data = Generate(count);
        // Дорогий запит: фільтр + сортування (буферизувальний оператор) + проєкція
        IEnumerable<StockLine> Query() => data.Where(p => p.Quantity < 100)
            .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit));

        int WithoutToList()
        {
            var q = Query();                      // перебирається тричі: кожен виклик — повне виконання
            return q.Count() + q.Sum(l => l.Quantity) + q.Max(l => l.Quantity);
        }
        int WithToList()
        {
            var list = Query().ToList();          // одне виконання
            return list.Count + list.Sum(l => l.Quantity) + list.Max(l => l.Quantity);
        }

        return
        [
            Measure("без ToList (запит перебирається 3 рази)", WithoutToList),
            Measure("з ToList (одне виконання)", WithToList),
        ];
    }

    private static BenchLine Measure(string name, Func<int> action)
    {
        action(); // прогрів JIT
        var times = new List<double>();
        int result = 0;
        for (int i = 0; i < 5; i++)
        {
            var sw = Stopwatch.StartNew();
            result = action();
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
        }
        times.Sort();
        return new BenchLine(name, times[times.Count / 2], result); // медіана з 5
    }
}
