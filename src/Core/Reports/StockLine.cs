namespace Core.Reports;

/// <summary>Рядок звіту: контракт не залежить від доменної сутності Product.</summary>
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);
