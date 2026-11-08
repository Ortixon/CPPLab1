namespace Core.Reports;

/// <summary>Зведення по товару: скільки переміщень і яка чиста зміна (результат GroupJoin).</summary>
public sealed record MovementSummary(string Sku, string Name, int Moves, int NetDelta);
