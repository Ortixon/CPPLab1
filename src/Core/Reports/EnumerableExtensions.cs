namespace Core.Reports;

public static class EnumerableExtensions
{
    /// <summary>
    /// Топ-N за цілим ключем (за спаданням) з обов'язковим другим ключем для стабільного порядку.
    /// Другий ключ змушує викликача його задати: топ-N без нього «стрибає» між запусками.
    /// </summary>
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, int> key, Func<T, string> tieBreaker, int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(tieBreaker);
        return source.OrderByDescending(key)
                     .ThenBy(tieBreaker, StringComparer.Ordinal)
                     .Take(Math.Max(0, n));
    }
}
