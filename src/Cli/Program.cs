using Core;
using Core.Reports;
using Core.Services;
using Core.Storage;


Console.OutputEncoding = System.Text.Encoding.UTF8;

var service = new CatalogService(StoreFactory.Create(args));
var items = service.All();
var movements = SampleData.Movements();

// Назва звіту — перший аргумент, що не є прапорцем і не є значенням прапорця (--file data.json).
var command = ParseCommand(args) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;
    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;
    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;
    case "report-empty":
        Console.WriteLine("Порожнє джерело:");
        Print(ReportService.LowStock([], 10));
        Print(ReportService.TopByQuantity([], 5));
        Console.WriteLine($" груп: {ReportService.ByUnit([]).Count}");
        var e = ReportService.Totals([]);
        Console.WriteLine($" позицій {e.Items}, всього {e.Total}, середнє {e.Average:F2}, макс {e.Max}");
        break;
    case "demo-lazy":
        var d = LazyDemo.Run(items);
        Console.WriteLine($"Дефіцитних до змін: {d.Before}");
        Console.WriteLine($"Дефіцитних після змін: {d.After}");
        Console.WriteLine($"У знімку лишилось: {d.SnapshotAfterClear} (знімок мав {d.SnapshotCount}, після Clear джерела не змінився)");
        break;
    case "report-query":
        foreach (var g in ReportService.ByUnitQuery(items))
            Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;
    case "report-moves":
        foreach (var m in ReportService.MovementLog(items, movements))
            Console.WriteLine($" {m.Date:yyyy-MM-dd}  {m.Sku,-10} {m.Name,-22} {m.Delta,5:+0;-0;0}");
        break;
    case "report-activity":
        foreach (var a in ReportService.Activity(items, movements))
            Console.WriteLine($" {a.Sku,-10} {a.Name,-22} переміщень: {a.Moves,2}  чиста зміна: {a.NetDelta,5:+0;-0;0}");
        break;
    case "report-hot":
        foreach (var a in ReportService.MostActive(items, movements, 5))
            Console.WriteLine($" {a.Sku,-10} {a.Name,-22} переміщень: {a.Moves,2}  чиста зміна: {a.NetDelta,5:+0;-0;0}");
        break;
    case "report-bench":
        foreach (var b in ReportBench.Run(100_000))
            Console.WriteLine($" {b.Variant,-45} {b.Milliseconds,8:F1} мс (контрольне значення {b.Result})");
        break;
    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum | report-empty | demo-lazy | report-query | report-moves | report-activity | report-hot | report-bench");
        break;
}

static string? ParseCommand(string[] args)
{
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--file") { i++; continue; } // пропускаємо прапорець і його значення
        if (!args[i].StartsWith("--")) return args[i];
    }
    return null;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    if (lines.Count == 0) { Console.WriteLine(" (немає записів)"); return; }
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}