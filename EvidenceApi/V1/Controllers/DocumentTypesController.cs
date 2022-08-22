using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/document_types")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class DocumentTypesController : BaseController
    {
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;

        public DocumentTypesController(
            ICreateAuditUseCase createAuditUseCase,
            IDocumentTypeGateway documentTypeGateway,
            IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway
        ) : base(createAuditUseCase)
        {
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
        }

        /// <summary>
        /// Returns all recognised document types by team name. Optional enabled? flag (bool) that returns documents that are enabled (true)
        /// or disabled (false) by the service area.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Team cannot be found</response>
        [HttpGet]
        [Route("{team}")]
        [ProducesResponseType(typeof(List<DocumentType>), StatusCodes.Status200OK)]
        public IActionResult GetDocumentTypesByTeamName([FromRoute][Required] string team, [FromQuery] bool? enabled = null)
        {
            var result = _documentTypeGateway.GetDocumentTypesByTeamName(team);

            if (result.Count > 0 && enabled.HasValue)
            {
                var resultEnabled = result.FindAll(dt => dt.Enabled == enabled);
                return Ok(resultEnabled);
            }

            if (result.Count > 0)
            {
                return Ok(result);
            }

            return NotFound($"No document types were found for team with name: {team}");
        }

        /// <summary>
        /// Returns all staff selected document types by team name
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Team cannot be found</response>
        [HttpGet]
        [Route("staff_selected/{team}")]
        [ProducesResponseType(typeof(List<DocumentType>), StatusCodes.Status200OK)]
        public IActionResult GetStaffSelectedDocumentTypesByTeamName([FromRoute][Required] string team)
        {
            var result = _staffSelectedDocumentTypeGateway.GetDocumentTypesByTeamName(team);
            if (result.Count > 0)
            {
                return Ok(result);
            }
            return NotFound($"No document types were found for team with name: {team}");
        }
    }
}
