using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
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
        [ProducesResponseType(typeof(List<DocumentType>), StatusCodes.Status200OK)]
        public IActionResult GetDocumentTypes()
        {
            return Ok(_gateway.GetAll());
        }
    }
}
