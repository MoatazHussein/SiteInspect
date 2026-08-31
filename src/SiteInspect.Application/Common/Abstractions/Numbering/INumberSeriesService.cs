namespace SiteInspect.Application.Common.Abstractions.Numbering;

public interface INumberSeriesService
{
    Task<string> NextAsync(
        NumberSeriesDefinition definition,
        CancellationToken cancellationToken = default);
}
