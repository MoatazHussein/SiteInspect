using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteInspect.Api.Authentication;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;

namespace SiteInspect.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.InspectionManager)]
[Route("api/dashboard")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}
