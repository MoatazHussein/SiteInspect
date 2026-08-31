using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Inspections.Commands.CreateInspection;

public sealed record CreateInspectionCommand(
    Guid ProjectId,
    Guid LocationId,
    Guid TemplateId,
    Guid InspectorId,
    DateTimeOffset DueAtUtc) : ICommand<CreateInspectionResponse>;
