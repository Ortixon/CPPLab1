namespace Core.Reports;

/// <summary>Рядок журналу переміщень (результат Join товарів і переміщень).</summary>
public sealed record MovementLine(string Sku, string Name, int Delta, DateOnly Date);
