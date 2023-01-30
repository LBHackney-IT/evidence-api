using System;
using System.Threading.Tasks;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers;

[ApiController]
[Route("api/v1/claims/backfill")]
[Produces("application/json")]
[ApiVersion("1.0")]

public class ClaimsBackfillController : BaseController
{
    private readonly IBackfillClaimTableWithResidentGroupIdUseCase _backfillClaimTableWithResidentGroupIdUseCase;
    public ClaimsBackfillController(ICreateAuditUseCase createAuditUseCase, IBackfillClaimTableWithResidentGroupIdUseCase backfillClaimTableWithResidentGroupIdUseCase) : base(createAuditUseCase)
    {
        _backfillClaimTableWithResidentGroupIdUseCase = backfillClaimTableWithResidentGroupIdUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> BackfillClaimsWithGroupId()
    {
        try
        {
            var result = await _backfillClaimTableWithResidentGroupIdUseCase.ExecuteAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error backfilling table - {ex}");
        }
    }

}
