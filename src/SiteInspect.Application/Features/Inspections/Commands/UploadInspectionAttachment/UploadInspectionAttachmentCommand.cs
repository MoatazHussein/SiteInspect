using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Inspections.Commands.UploadInspectionAttachment;

public sealed record UploadInspectionAttachmentCommand(
    Guid InspectionId,
    Guid ObservationId,
    string RowVersion,
    string FileName,
    string ContentType,
    long Length,
    Stream Content) : ICommand<UploadInspectionAttachmentResponse>;
