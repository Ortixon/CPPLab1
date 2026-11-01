using Core.Abstractions;
using Core.Events;

namespace Cli;

// Додаткове завдання 1: реалізації INotificationSink живуть у Cli, а не в Core.
public static class LogFormat
{
    public static string Line(CatalogChangedEventArgs e) =>
        $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
}

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e) => Console.WriteLine("[console] " + LogFormat.Line(e));
}

public sealed class FileSink(string path) : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e) =>
        File.AppendAllText(path, LogFormat.Line(e) + Environment.NewLine);
}

public sealed class CompositeSink(params INotificationSink[] sinks) : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        foreach (var s in sinks) s.Notify(e);
    }
}
