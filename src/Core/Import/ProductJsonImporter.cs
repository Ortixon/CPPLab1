using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

// Додаткове завдання 1: другий імпортер (JSON) на тих самих типах.
// Формат: масив об'єктів [{ "id": ..., "sku": ..., "name": ..., "unit": ..., "quantity": ..., "note": ... }],
// кодування UTF-8, імена полів без урахування регістру.
public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path, Encoding.UTF8);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            errors.Add($"файл не є коректним JSON: {ex.Message}");
            return new ImportResult<ProductDto>(items, errors);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("очікую масив об'єктів на верхньому рівні");
                return new ImportResult<ProductDto>(items, errors);
            }

            int number = 0;
            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                number++;
                switch (ParseElement(element))
                {
                    case ParseOk ok:
                        items.Add(ok.Value);
                        break;
                    case ParseFailed failed:
                        errors.Add($"елемент {number}: {failed.Reason}");
                        break;
                }
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseElement(JsonElement element)
    {
        ProductDto? dto;
        try
        {
            dto = element.Deserialize<ProductDto>(Options);
        }
        catch (JsonException ex)
        {
            return new ParseFailed($"некоректне значення поля {ex.Path}");
        }

        return dto switch
        {
            null => new ParseFailed("порожній елемент (null)"),
            { Id: null or "" } => new ParseFailed("відсутній id"),
            { Sku: null or "" } or { Name: null or "" } => new ParseFailed("SKU або назва порожні"),
            { Quantity: < 0 } => new ParseFailed($"кількість '{dto.Quantity}' не є невід'ємним числом"),
            _ => new ParseOk(dto)
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
