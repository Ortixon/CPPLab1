namespace Core.Dto;

public sealed record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<WarehouseDto> Warehouses,
    IReadOnlyList<string> Errors)
{
    public ImportStats Stats =>
        new(Products.Count + Warehouses.Count + Errors.Count,
            Products.Count + Warehouses.Count,
            Errors.Count);
}
