using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

// Додаткове завдання 2: різнорідні рядки за префіксом типу.
// "P;id;sku;name;unit;quantity" — товар, "W;id;name;address" — склад (address необов'язковий).
// Один switch, два різні типи результату.
public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParsedProduct p:
                    products.Add(p.Value);
                    break;
                case ParsedWarehouse w:
                    warehouses.Add(w.Value);
                    break;
                case ParseFailed f:
                    errors.Add($"рядок {number}: {f.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P" or "p", .. var rest] when rest.Length != 5
                => new ParseFailed($"товар: очікую 5 полів після префікса, отримав {rest.Length}"),
            ["P" or "p", _, "", _, _, _] or ["P" or "p", _, _, "", _, _]
                => new ParseFailed("товар: SKU або назва порожні"),
            ["P" or "p", var id, var sku, var name, var unit, var qty]
                when int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) && q >= 0
                => new ParsedProduct(new ProductDto(id, sku, name, unit, q)),
            ["P" or "p", .., var qty]
                => new ParseFailed($"товар: кількість '{qty}' не є невід'ємним числом"),
            ["W" or "w", _, ""] or ["W" or "w", _, "", _]
                => new ParseFailed("склад: назва порожня"),
            ["W" or "w", var id, var name]
                => new ParsedWarehouse(new WarehouseDto(id, name)),
            ["W" or "w", var id, var name, var address]
                => new ParsedWarehouse(new WarehouseDto(id, name, address == "" ? null : address)),
            ["W" or "w", ..]
                => new ParseFailed($"склад: очікую id;назва[;адреса], отримав {parts.Length - 1} полів"),
            [var prefix, ..]
                => new ParseFailed($"невідомий префікс типу '{prefix}'"),
            _ => new ParseFailed("порожній рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParsedProduct(ProductDto Value) : ParseOutcome;
    private sealed record ParsedWarehouse(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
