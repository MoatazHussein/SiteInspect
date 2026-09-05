using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SiteInspect.Application.Common.Abstractions.Storage;

namespace SiteInspect.Infrastructure.Storage;

internal sealed class LocalInspectionAttachmentStorage : IInspectionAttachmentStorage
{
    private readonly string storageRoot;

    public LocalInspectionAttachmentStorage(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        var configuredPath = configuration["Storage:AttachmentsPath"];
        storageRoot = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(hostEnvironment.ContentRootPath, "App_Data", "attachments")
            : Path.GetFullPath(configuredPath, hostEnvironment.ContentRootPath);
        Directory.CreateDirectory(storageRoot);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var storedFileName = $"{Guid.CreateVersion7():N}{fileExtension}";
        var filePath = GetSafePath(storedFileName);

        try
        {
            await using var destination = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await content.CopyToAsync(destination, cancellationToken);
        }
        catch
        {
            File.Delete(filePath);
            throw;
        }

        return storedFileName;
    }

    public Task<Stream?> OpenReadAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetSafePath(storedFileName);
        Stream? content = File.Exists(filePath)
            ? new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)
            : null;

        return Task.FromResult(content);
    }

    public Task DeleteAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetSafePath(storedFileName);
        File.Delete(filePath);
        return Task.CompletedTask;
    }

    private string GetSafePath(string storedFileName)
    {
        if (Path.GetFileName(storedFileName) != storedFileName)
        {
            throw new InvalidOperationException("The stored attachment name is invalid.");
        }

        return Path.Combine(storageRoot, storedFileName);
    }
}
