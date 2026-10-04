using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

bool mixed = args.Length > 0 && args[0] == "--mixed";
string[] rest = mixed ? args[1..] : args;

string path = rest.Length > 0
    ? rest[0]
    : Path.Combine("data", mixed ? "mixed.csv" : "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

if (mixed)
{
    MixedImportResult mixedResult = MixedCsvImporter.Load(path);

    Console.WriteLine($"Завантажено товарів: {mixedResult.Products.Count}, складів: {mixedResult.Warehouses.Count}");
    foreach (ProductDto p in mixedResult.Products.Take(5))
        Console.WriteLine($"  товар {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
    foreach (WarehouseDto w in mixedResult.Warehouses.Take(5))
        Console.WriteLine($"  склад {w.Id,-6} {w.Name,-26} {w.Address ?? "—"}");

    PrintErrors(mixedResult.Errors);
    Console.WriteLine($"Статистика: {mixedResult.Stats}");
    return 0;
}

// Вибір імпортера за розширенням файлу — switch expression.
Func<string, ImportResult<ProductDto>>? importer = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load,
    ".json" => ProductJsonImporter.Load,
    _ => null
};

if (importer is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу '{Path.GetExtension(path)}'. Очікую .csv або .json.");
    return 2;
}

ImportResult<ProductDto> result = importer(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($"  {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");

PrintErrors(result.Errors);
Console.WriteLine($"Статистика: {result.Stats}");

return 0;

static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine($"Пропущено рядків: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($"  ! {e}");
}
