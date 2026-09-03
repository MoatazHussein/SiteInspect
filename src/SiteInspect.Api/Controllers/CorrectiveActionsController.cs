using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteInspect.Api.Authentication;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Features.CorrectiveActions.Commands.ApproveCorrectiveAction;
using SiteInspect.Application.Features.CorrectiveActions.Commands.CreateCorrectiveAction;
using SiteInspect.Application.Features.CorrectiveActions.Commands.RejectCorrectiveAction;
using SiteInspect.Application.Features.CorrectiveActions.Commands.SubmitCorrectiveActionResponse;
using SiteInspect.Application.Features.CorrectiveActions.Queries.GetAssignedCorrectiveActions;
using SiteInspect.Application.Features.CorrectiveActions.Queries.GetContractors;
using SiteInspect.Application.Features.CorrectiveActions.Queries.GetCorrectiveActions;

namespace SiteInspect.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/corrective-actions")]
public sealed class CorrectiveActionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCorrectiveActionCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext, StatusCodes.Status201Created);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> List(
        [FromQuery] GetCorrectiveActionsQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpGet("contractors")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> GetContractors(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContractorsQuery(), cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpGet("mine")]
    [Authorize(Policy = AuthorizationPolicies.CorrectiveActionContractor)]
    public async Task<IActionResult> GetAssigned(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAssignedCorrectiveActionsQuery(), cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpPost("respond")]
    [Authorize(Policy = AuthorizationPolicies.CorrectiveActionContractor)]
    public async Task<IActionResult> SubmitResponse(
        [FromBody] SubmitCorrectiveActionResponseCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpPost("approve")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> Approve(
        [FromBody] ApproveCorrectiveActionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    [HttpPost("reject")]
    [Authorize(Policy = AuthorizationPolicies.InspectionManager)]
    public async Task<IActionResult> Reject(
        [FromBody] RejectCorrectiveActionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}

