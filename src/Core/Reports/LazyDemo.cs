using Core.Domain;

namespace Core.Reports;

public sealed record LazyDemoResult(int Before, int After, int SnapshotCount, int SnapshotAfterClear);

/// <summary>
/// Демонстрація відкладеного виконання. Лежить у Core, щоб у Program.cs не було жодного Where.
/// Джерело — локальний список: service.All() щоразу повертає новий знімок.
/// </summary>
public static class LazyDemo
{
    public static LazyDemoResult Run(IEnumerable<Product> items, int threshold = 10)
    {
        var source = items.ToList();
        var query = source.Where(p => p.Quantity < threshold);   // запит побудовано, не виконано
        int before = query.Count();                              // 1-й запуск запиту

        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));
        int after = query.Count();                               // 2-й запуск: нове джерело → інше число

        var snapshot = query.ToList();                           // результат зафіксовано
        int snapCount = snapshot.Count;
        source.Clear();
        return new LazyDemoResult(before, after, snapCount, snapshot.Count); // snapshot не змінився
    }
}
