using System.Globalization;
using SiteInspect.Application.Common.Abstractions.Numbering;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Domain.Common.Numbering;

namespace SiteInspect.Application.Common.Numbering;

public sealed class NumberSeriesService(
    IRepository<NumberSeriesCounter> counterRepository,
    TimeProvider timeProvider) : INumberSeriesService
{
    public async Task<string> NextAsync(
        NumberSeriesDefinition definition,
        CancellationToken cancellationToken = default)
    {
        Validate(definition);
        var year = timeProvider.GetUtcNow().Year;
        var counter = await counterRepository.FirstOrDefaultAsync(
            item => item.SeriesName == definition.Name && item.Year == year,
            cancellationToken);

        if (counter is null)
        {
            counter = NumberSeriesCounter.Create(definition.Name, year);
            var serialNumber = counter.IssueNext();
            await counterRepository.AddAsync(counter, cancellationToken);

            return Format(definition, year, serialNumber);
        }

        var nextSerialNumber = counter.IssueNext();
        counterRepository.Update(counter);

        return Format(definition, year, nextSerialNumber);
    }

    private static string Format(
        NumberSeriesDefinition definition,
        int year,
        long serialNumber) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{definition.Prefix}-{year}-{serialNumber.ToString($"D{definition.PaddingLength}", CultureInfo.InvariantCulture)}");

    private static void Validate(NumberSeriesDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Prefix);

        if (definition.PaddingLength is < 1 or > 18)
        {
            throw new ArgumentOutOfRangeException(nameof(definition));
        }
    }
}
