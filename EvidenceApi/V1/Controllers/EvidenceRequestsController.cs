using System;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EvidenceApi.V1.Gateways.Interfaces;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/evidence_requests")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class EvidenceRequestsController : BaseController
    {
        private readonly ICreateEvidenceRequestUseCase _creator;
        private readonly ICreateDocumentSubmissionUseCase _createDocumentSubmission;
        private readonly IFindEvidenceRequestByIdUseCase _evidenceRequestUseCase;
        private readonly IFindEvidenceRequestsUseCase _getEvidenceRequestsUseCase;
        private readonly IDocumentsApiGateway _documentsApiGateway;

        public EvidenceRequestsController(
            ICreateEvidenceRequestUseCase creator,
            ICreateDocumentSubmissionUseCase createDocumentSubmission,
            IFindEvidenceRequestByIdUseCase evidenceRequestUseCase,
            IFindEvidenceRequestsUseCase getEvidenceRequestsUseCase,
            ICreateAuditUseCase createAuditUseCase,
            IDocumentsApiGateway documentsApiGateway
        ) : base(createAuditUseCase)
        {
            _creator = creator;
            _createDocumentSubmission = createDocumentSubmission;
            _evidenceRequestUseCase = evidenceRequestUseCase;
            _getEvidenceRequestsUseCase = getEvidenceRequestsUseCase;
            _documentsApiGateway = documentsApiGateway;
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
                return StatusCode(400, ex.Message);
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
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Calls documents api to create a pre-signed upload url
        /// </summary>
        /// <response code="201">Saved</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="401">Request lacks valid API token</response>
        /// <response code="404">Evidence request cannot be found</response>
        [HttpGet]
        [Route("generate_upload_url")]
        public async Task<IActionResult> CallDocumentsApiToGenerateUploadUrl()
        {
            try
            {
                var result = await _documentsApiGateway.CreateUploadPolicy().ConfigureAwait(true);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
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
        public async Task<IActionResult> CreateDocumentSubmission([FromRoute][Required] Guid evidenceRequestId, [FromBody][Required] DocumentSubmissionRequest request)
        {
            try
            {
                var result = await _createDocumentSubmission.ExecuteAsync(evidenceRequestId, request).ConfigureAwait(true);
                return Created(new Uri($"/evidence_requests/{evidenceRequestId}/document_submissions", UriKind.Relative), result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Finds evidence request
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="400">Request contains invalid parameters</response>
        [HttpGet]
        public IActionResult FilterEvidenceRequests([FromQuery][Required] EvidenceRequestsSearchQuery request)
        {
            try
            {
                var result = _getEvidenceRequestsUseCase.Execute(request);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
