using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteInspect.Api.Authentication;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Features.Inspections.Commands.CancelInspection;
using SiteInspect.Application.Features.Inspections.Commands.CreateInspection;
using SiteInspect.Application.Features.Inspections.Commands.ReassignInspection;
using SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;
using SiteInspect.Application.Features.Inspections.Commands.StartInspection;
using SiteInspect.Application.Features.Inspections.Queries.GetInspection;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionManagementOptions;
using SiteInspect.Application.Features.Inspections.Queries.GetInspections;

namespace SiteInspect.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.InspectionReader)]
[Route("api/inspections")]
public sealed class InspectionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> CreateInspection(
        [FromBody] CreateInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext, StatusCodes.Status201Created);
    }

    [HttpGet]
    public async Task<IActionResult> GetInspections(
        [FromQuery] GetInspectionsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpGet("filter-options")]
    public async Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInspectionFilterOptionsQuery(),
            cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpGet("management-options")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> GetManagementOptions(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInspectionManagementOptionsQuery(),
            cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpPut("reassign")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> ReassignInspection(
        [FromBody] ReassignInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpPost("cancel")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> CancelInspection(
        [FromBody] CancelInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpPost("start")]
    [Authorize(Policy = AuthorizationPolicies.InspectionExecutor)]
    public async Task<IActionResult> StartInspection(
        [FromBody] StartInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpPut("draft")]
    [Authorize(Policy = AuthorizationPolicies.InspectionExecutor)]
    public async Task<IActionResult> SaveInspectionDraft(
        [FromBody] SaveInspectionDraftCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpGet("{inspectionId:guid}")]
    public async Task<IActionResult> GetInspection(
        [FromRoute] Guid inspectionId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetInspectionQuery(inspectionId),
            cancellationToken);

        return result.ToActionResult(HttpContext);
    }
}
