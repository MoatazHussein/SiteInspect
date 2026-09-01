using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteInspect.Api.Authentication;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Commands.CancelInspection;
using SiteInspect.Application.Features.Inspections.Commands.CreateInspection;
using SiteInspect.Application.Features.Inspections.Commands.ReassignInspection;
using SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;
using SiteInspect.Application.Features.Inspections.Commands.StartInspection;
using SiteInspect.Application.Features.Inspections.Commands.SubmitInspection;
using SiteInspect.Application.Features.Inspections.Commands.UploadInspectionAttachment;
using SiteInspect.Application.Features.Inspections.Queries.DownloadInspectionAttachment;
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

    [[HttpPost("attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [Authorize(Policy = AuthorizationPolicies.InspectionExecutor)]
    public async Task<IActionResult> UploadInspectionAttachment(
    [FromForm] Guid inspectionId,
    [FromForm] Guid observationId,
    [FromForm] string rowVersion,
    [FromForm] IFormFile? file,
    CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return Result<UploadInspectionAttachmentResponse>
                .Failure(CommonErrors.Validation)
                .ToActionResult(HttpContext);
        }

        await using var content = file.OpenReadStream();

        var result = await sender.Send(
            new UploadInspectionAttachmentCommand(
                inspectionId,
                observationId,
                rowVersion,
                file.FileName,
                file.ContentType,
                file.Length,
                content),
            cancellationToken);

        return result.ToActionResult(
            HttpContext,
            StatusCodes.Status201Created);
    }

    [HttpPost("submit")]
    [Authorize(Policy = AuthorizationPolicies.InspectionExecutor)]
    public async Task<IActionResult> SubmitInspection(
        [FromBody] SubmitInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    [HttpGet("{inspectionId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadInspectionAttachment(
        [FromRoute] Guid inspectionId,
        [FromRoute] Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DownloadInspectionAttachmentQuery(inspectionId, attachmentId),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(HttpContext);
        }

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
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
