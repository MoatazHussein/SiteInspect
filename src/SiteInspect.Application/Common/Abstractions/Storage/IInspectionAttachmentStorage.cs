namespace SiteInspect.Application.Common.Abstractions.Storage;

public interface IInspectionAttachmentStorage
{
    Task<string> SaveAsync(
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storedFileName,
        CancellationToken cancellationToken = default);
}
