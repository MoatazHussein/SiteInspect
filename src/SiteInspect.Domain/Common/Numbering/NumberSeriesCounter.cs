using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Common.Numbering;

public sealed class NumberSeriesCounter : ConcurrentAuditableEntity
{
    private NumberSeriesCounter()
    {
    }

    private NumberSeriesCounter(string seriesName, int year, long lastIssuedValue)
    {
        SeriesName = seriesName;
        Year = year;
        LastIssuedValue = lastIssuedValue;
    }

    public string SeriesName { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public long LastIssuedValue { get; private set; }

    public static NumberSeriesCounter Create(string seriesName, int year, long lastIssuedValue = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesName);

        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(lastIssuedValue);

        return new NumberSeriesCounter(seriesName.Trim(), year, lastIssuedValue);
    }

    public long IssueNext()
    {
        LastIssuedValue = checked(LastIssuedValue + 1);
        return LastIssuedValue;
    }

    public void AdvanceTo(long issuedValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(issuedValue);

        if (issuedValue > LastIssuedValue)
        {
            LastIssuedValue = issuedValue;
        }
    }
}
