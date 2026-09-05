using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Numbering;
using SiteInspect.Domain.Common.Numbering;

namespace SiteInspect.Infrastructure.Persistence.Initialization.Steps;

internal sealed class InspectionNumberCountersSeedStep(ApplicationDbContext dbContext, TimeProvider timeProvider) : ISeedStep
{
    public string Name => "Inspection number counters";
    public int Order => SeedStepOrder.InspectionNumberCounters;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var currentYear = timeProvider.GetUtcNow().Year;
        var definition = NumberSeriesDefinitions.Inspections;
        var inspectionNumbers = await dbContext.Inspections
            .AsNoTracking()
            .Select(inspection => inspection.Number)
            .ToArrayAsync(cancellationToken);

        var lastIssuedByYear = new Dictionary<int, long>();
        foreach (var number in inspectionNumbers)
        {
            if (!TryParseNumber(number, definition.Prefix, out var year, out var serialNumber))
            {
                continue;
            }

            if (!lastIssuedByYear.TryGetValue(year, out var currentValue) || serialNumber > currentValue)
            {
                lastIssuedByYear[year] = serialNumber;
            }
        }

        lastIssuedByYear.TryAdd(currentYear, 0);

        var years = lastIssuedByYear.Keys.ToArray();
        var existingCounters = await dbContext.NumberSeriesCounters
            .Where(counter => counter.SeriesName == definition.Name && years.Contains(counter.Year))
            .ToDictionaryAsync(counter => counter.Year, cancellationToken);

        foreach (var (year, lastIssuedValue) in lastIssuedByYear)
        {
            if (existingCounters.TryGetValue(year, out var counter))
            {
                counter.AdvanceTo(lastIssuedValue);
            }
            else
            {
                dbContext.NumberSeriesCounters.Add(
                    NumberSeriesCounter.Create(definition.Name, year, lastIssuedValue));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseNumber(
        string number,
        string expectedPrefix,
        out int year,
        out long serialNumber)
    {
        year = 0;
        serialNumber = 0;
        var parts = number.Split('-', StringSplitOptions.TrimEntries);

        return parts.Length == 3 &&
            string.Equals(parts[0], expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(parts[1], out year) &&
            long.TryParse(parts[2], out serialNumber) &&
            year > 0 &&
            serialNumber >= 0;
    }
}
