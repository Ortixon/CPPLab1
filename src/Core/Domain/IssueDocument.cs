namespace Core.Domain;

// Документ видачі зі складу. Інваріанти:
//  1. Id і отримувач не порожні;
//  2. кількість у рядку > 0;
//  3. рядки додаються лише до чернетки (Draft);
//  4. не можна підтвердити порожній документ;
//  5. зміна статусу лише за дозволеними переходами (DocumentStatusRules).
public sealed class IssueDocument
{
    private readonly List<DocumentLine> _lines = [];

    public string Id { get; }
    public string RecipientId { get; }
    public DocumentStatus Status { get; private set; }
    public IReadOnlyList<DocumentLine> Lines => _lines.AsReadOnly();

    private IssueDocument(string id, string recipientId)
    {
        Id = id;
        RecipientId = recipientId;
        Status = DocumentStatus.Draft;
    }

    public static IssueDocument Create(string id, string recipientId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер документа обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(recipientId))
            throw new ArgumentException("Отримувач обов'язковий", nameof(recipientId));
        return new IssueDocument(id.Trim(), recipientId.Trim());
    }

    public void AddLine(string productId, int quantity)
    {
        if (Status != DocumentStatus.Draft)
            throw new InvalidOperationException(
                $"Документ {Id} має статус {Status}, рядки додавати не можна");
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Кількість у рядку має бути більшою за нуль");
        _lines.Add(new DocumentLine(productId.Trim(), quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Документ {Id} порожній, підтверджувати нічого");
        MoveTo(DocumentStatus.Confirmed);
    }

    public void Cancel() => MoveTo(DocumentStatus.Cancelled);

    private void MoveTo(DocumentStatus next)
    {
        if (!DocumentStatusRules.CanTransition(Status, next))
            throw new InvalidOperationException(
                $"Документ {Id}: перехід {Status} → {next} неможливий");
        Status = next;
    }

    public override string ToString() => $"{Id} для {RecipientId}: {Status}, рядків {_lines.Count}";
}
