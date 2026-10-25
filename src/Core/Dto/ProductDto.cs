namespace Core.Dto;

// Record з лабораторної 3 — формат даних (рядок файлу, запис у сховищі).
// Нічого не гарантує, тому тут можливий і порожній SKU, і від'ємна кількість.
public sealed record ProductDto(
    string Id,
    string Sku,
    string Name,
    string Unit,
    int Quantity,
    string? Note = null);
