using System.Collections.Generic;
using EvidenceApi.V1.UseCase;
using Microsoft.AspNetCore.Mvc;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.Controllers
{
    [Route("api/v1/healthcheck")]
    [ApiController]
    [Produces("application/json")]
    public class HealthCheckController : BaseController
    {
        public HealthCheckController(ICreateAuditUseCase createAuditUseCase) : base(createAuditUseCase)
        {
        }

        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }

        [HttpGet]
        [Route("error")]
        public void ThrowError()
        {
            ThrowOpsErrorUsecase.Execute();
        }

    }
}
