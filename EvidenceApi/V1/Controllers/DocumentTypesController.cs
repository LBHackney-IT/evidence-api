using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/document_types")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class DocumentTypesController : BaseController
    {
        private readonly IDocumentTypeGateway _gateway;
        public DocumentTypesController(IDocumentTypeGateway gateway)
        {
            _gateway = gateway;
        }

        /// <summary>
        /// Returns all recognised document types
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseObjectList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetDocumentTypes()
        {
            return Ok(_gateway.GetAll());
        }
    }
}
