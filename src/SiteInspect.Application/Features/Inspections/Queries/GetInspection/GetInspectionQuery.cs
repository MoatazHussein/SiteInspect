using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed record GetInspectionQuery(Guid InspectionId) : IQuery<GetInspectionResponse>;
