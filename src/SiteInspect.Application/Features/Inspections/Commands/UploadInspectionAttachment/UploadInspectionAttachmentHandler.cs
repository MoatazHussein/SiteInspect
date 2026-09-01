using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Abstractions.Storage;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.UploadInspectionAttachment;

public sealed class UploadInspectionAttachmentHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IInspectionAttachmentStorage attachmentStorage,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<UploadInspectionAttachmentCommand, Result<UploadInspectionAttachmentResponse>>
{
    public async Task<Result<UploadInspectionAttachmentResponse>> Handle(
        UploadInspectionAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Inspector) || currentUser.UserId is not Guid inspectorId)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetWithObservationsForUpdateAsync(
            command.InspectionId,
            RowVersionToken.Decode(command.RowVersion),
            cancellationToken);

        if (inspection is null)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.NotFound(command.InspectionId));
        }

        if (inspection.AssignedInspectorId != inspectorId)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.AssignedInspectorRequired);
        }

        if (inspection.Status != InspectionStatus.InProgress)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.AttachmentNotAllowed);
        }

        var observation = inspection.Observations.SingleOrDefault(item => item.Id == command.ObservationId);
        if (observation is null)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.ObservationNotFound(command.ObservationId));
        }

        if (observation.Attachments.Count >= 5)
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.AttachmentLimitReached);
        }

        if (!await HasValidImageSignatureAsync(
            command.Content,
            command.ContentType,
            cancellationToken))
        {
            return Result<UploadInspectionAttachmentResponse>.Failure(
                InspectionErrors.InvalidAttachmentContent);
        }

        var extension = GetExtension(command.ContentType);
        var storedFileName = await attachmentStorage.SaveAsync(
            command.Content,
            extension,
            cancellationToken);

        var databasePersisted = false;
        try
        {
            var uploadedAtUtc = timeProvider.GetUtcNow();
            var attachment = inspection.AddAttachment(
                inspectorId,
                command.ObservationId,
                Path.GetFileName(command.FileName),
                storedFileName,
                command.ContentType.ToLowerInvariant(),
                command.Length,
                uploadedAtUtc);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            databasePersisted = true;

            return Result<UploadInspectionAttachmentResponse>.Success(
                new UploadInspectionAttachmentResponse(
                    attachment.Id,
                    attachment.ObservationId,
                    attachment.OriginalFileName,
                    attachment.ContentType,
                    attachment.Length,
                    attachment.CreatedAtUtc,
                    Convert.ToBase64String(inspection.RowVersion)));
        }
        finally
        {
            if (!databasePersisted)
            {
                await attachmentStorage.DeleteAsync(storedFileName, CancellationToken.None);
            }
        }
    }

    private static string GetExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new InvalidOperationException("Unsupported image content type."),
    };

    private static async Task<bool> HasValidImageSignatureAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (!content.CanSeek)
        {
            return false;
        }

        var originalPosition = content.Position;
        var header = new byte[12];
        var bytesRead = await content.ReadAsync(header, cancellationToken);
        content.Position = originalPosition;

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => bytesRead >= 3 &&
                header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            "image/png" => bytesRead >= 8 &&
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            "image/webp" => bytesRead >= 12 &&
                header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false,
        };
    }
}
