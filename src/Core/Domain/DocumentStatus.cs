namespace Core.Domain;

// Додаткове завдання 3: явний стан документа видачі.
public enum DocumentStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public static class DocumentStatusRules
{
    // Допустимі переходи: Draft → Confirmed, Draft → Cancelled, Confirmed → Cancelled.
    // Cancelled — кінцевий стан. Решта — заборонено.
    public static bool CanTransition(DocumentStatus from, DocumentStatus to) => (from, to) switch
    {
        (DocumentStatus.Draft, DocumentStatus.Confirmed) => true,
        (DocumentStatus.Draft, DocumentStatus.Cancelled) => true,
        (DocumentStatus.Confirmed, DocumentStatus.Cancelled) => true,
        _ => false
    };
}
