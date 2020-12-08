using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/evidence_requests")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class EvidenceRequestsController : BaseController
    {
        private readonly IDocumentTypeGateway _gateway;
        public EvidenceRequestsController(IDocumentTypeGateway gateway)
        {
            _gateway = gateway;
        }

        /// <summary>
        /// Creates a new evidence request
        /// </summary>
        /// <response code="201">Saved</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="401">Request lacks valid API token</response>
        [HttpPost]
        public IActionResult CreateEvidenceRequest([FromBody] EvidenceRequestRequest request)
        {
            // TODO: Implement this controller action
            return Created(new Uri("/evidence_requests/123", UriKind.Relative), request);
        }
    }
}
