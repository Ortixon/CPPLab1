using Core.Dto;

namespace Core.Domain;

// Додаткове завдання 1: «дані + помилки» на рівні сутностей.
public sealed record DomainImportResult(
    IReadOnlyList<Product> Products,
    IReadOnlyList<string> Errors)
{
    // Бере результат імпорту тижня 3 і пропускає кожен DTO через інваріанти домену.
    // Рядки, відхилені на етапі розбору файлу, переходять у загальний перелік помилок.
    public static DomainImportResult FromImport(ImportResult<ProductDto> import)
    {
        List<Product> products = [];
        List<string> errors = [.. import.Errors];

        foreach (ProductDto dto in import.Items)
        {
            try
            {
                products.Add(Product.FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                errors.Add($"товар '{dto.Id}' (SKU '{dto.Sku}'): {ex.Message}");
            }
        }

        return new DomainImportResult(products, errors);
    }
}
