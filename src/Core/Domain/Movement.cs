namespace Core.Domain;

/// <summary>Переміщення товару: ProductId посилається на Product.Id, Delta &gt; 0 — прихід, &lt; 0 — видача.</summary>
public sealed record Movement(string ProductId, int Delta, DateOnly Date);
