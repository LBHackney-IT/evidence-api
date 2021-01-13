using System;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
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
        private readonly ICreateEvidenceRequestUseCase _creator;
        private readonly ICreateDocumentSubmissionUseCase _createDocumentSubmission;
        private readonly IFindEvidenceRequestUseCase _evidenceRequestUseCase;

        public EvidenceRequestsController(IDocumentTypeGateway gateway, ICreateEvidenceRequestUseCase creator, ICreateDocumentSubmissionUseCase createDocumentSubmission, IFindEvidenceRequestUseCase evidenceRequestUseCase)
        {
            _gateway = gateway;
            _creator = creator;
            _createDocumentSubmission = createDocumentSubmission;
            _evidenceRequestUseCase = evidenceRequestUseCase;
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
            try
            {
                var result = _creator.Execute(request);
                return Created(new Uri($"/evidence_requests/{result.Id}", UriKind.Relative), result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.ValidationResponse.Errors);
            }
            catch (NotificationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Finds evidence request
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="401">Request lacks valid API token</response>
        /// <response code="404">Resource not found</response>
        [HttpGet]
        [Route("{id}")]
        public IActionResult FindEvidenceRequest([FromRoute][Required] Guid id)
        {
            try
            {
                var result = _evidenceRequestUseCase.Execute(id);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Error);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Creates a new document submission
        /// </summary>
        /// <response code="201">Saved</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="401">Request lacks valid API token</response>
        /// <response code="404">Evidence request cannot be found</response>
        [HttpPost]
        [Route("{evidenceRequestId}/document_submissions")]
        public IActionResult CreateDocumentSubmission([FromRoute][Required] Guid evidenceRequestId, [FromBody][Required] DocumentSubmissionRequest request)
        {
            try
            {
                var result = _createDocumentSubmission.Execute(evidenceRequestId, request);
                return Created(new Uri($"/evidence_requests/{evidenceRequestId}/document_submissions", UriKind.Relative), result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Error);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
