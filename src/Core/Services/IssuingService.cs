using Core.Domain;

namespace Core.Services;

// Додаткове завдання 2: правила, що охоплюють ДВІ і більше сутностей.
//  - отримувач не може мати більше MaxOpenDocuments чернеток одночасно
//    (треба знати ВСІ документи — окремий IssueDocument їх не бачить);
//  - підтвердження видачі перевіряє залишки ВСІХ товарів до списання
//    (документ не знає залишків, Product не знає документа).
public sealed class IssuingService
{
    public const int MaxOpenDocuments = 5;

    private readonly Dictionary<string, Product> _products;
    private readonly List<IssueDocument> _documents = [];

    public IssuingService(IEnumerable<Product> products) =>
        _products = products.ToDictionary(p => p.Id);

    public IReadOnlyList<IssueDocument> Documents => _documents.AsReadOnly();

    public IssueDocument OpenDocument(string documentId, string recipientId)
    {
        int open = _documents.Count(d => d.RecipientId == recipientId.Trim()
                                         && d.Status == DocumentStatus.Draft);
        if (open >= MaxOpenDocuments)
            throw new InvalidOperationException(
                $"Отримувач {recipientId} уже має {open} відкритих документів (ліміт {MaxOpenDocuments})");

        IssueDocument doc = IssueDocument.Create(documentId, recipientId);
        _documents.Add(doc);
        return doc;
    }

    public void ConfirmIssue(IssueDocument doc)
    {
        // 1. Перевіряємо всі рядки ДО першої зміни стану.
        foreach (var group in doc.Lines.GroupBy(l => l.ProductId))
        {
            if (!_products.TryGetValue(group.Key, out Product? product))
                throw new InvalidOperationException($"Товар {group.Key} не знайдено на складі");

            int needed = group.Sum(l => l.Quantity);
            if (needed > product.Quantity)
                throw new InvalidOperationException(
                    $"Документ {doc.Id}: для {product.Sku} потрібно {needed}, на складі {product.Quantity}");
        }

        // 2. Статус (кине виняток для порожнього або вже підтвердженого документа).
        doc.Confirm();

        // 3. Списання: після перевірки воно не може зірватися посередині.
        foreach (DocumentLine line in doc.Lines)
            _products[line.ProductId].Issue(line.Quantity);
    }
}
