using Core.Events;

namespace Core.Abstractions;

// Додаткове завдання 1: альтернатива події — приймач сповіщень, який передають у конструктор.
public interface INotificationSink
{
    void Notify(CatalogChangedEventArgs e);
}
