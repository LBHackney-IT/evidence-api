using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers;

[ApiController]
[Route("api/v1/claims/backfill")]
[Produces("application/json")]
[ApiVersion("1.0")]

public class ClaimsBackfillController : BaseController
{
    public ClaimsBackfillController(ICreateAuditUseCase createAuditUseCase) : base(createAuditUseCase)
    {
    }

    [HttpGet]
    public IActionResult BackfillClaimsWithGroupId()
    {
        try
        {
            return Ok("Not yet implemented");
        }
        catch
        {
            return BadRequest("bad request");
        }
    }

}
