using System.Globalization;

namespace Core.Dto;

public sealed record ImportStats(int Total, int Accepted, int Skipped)
{
    public double ErrorPercent => Total == 0 ? 0 : 100.0 * Skipped / Total;

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture,
            $"усього: {Total} / прийнято: {Accepted} / пропущено: {Skipped} / помилок: {ErrorPercent:F1}%");
}
