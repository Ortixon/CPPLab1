namespace Core.Reports;

/// <summary>Підсумок за групою (одиниця виміру): ключ, кількість позицій, сумарний залишок.</summary>
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);
