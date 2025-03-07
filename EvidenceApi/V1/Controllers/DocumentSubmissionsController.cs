using System;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/document_submissions")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class DocumentSubmissionsController : BaseController
    {
        private readonly IUpdateDocumentSubmissionStateUseCase _updateDocumentSubmissionStateUseCase;
        private readonly IUpdateDocumentSubmissionVisibiltyUseCase _updateDocumentSubmissionVisibilityUseCase;
        private readonly IFindDocumentSubmissionByIdUseCase _findDocumentSubmissionByIdUseCase;
        private readonly IFindDocumentSubmissionsByResidentIdUseCase _findDocumentSubmissionsByResidentIdUseCase;
        private readonly ICreateDocumentSubmissionWithoutEvidenceRequestUseCase _createDocumentSubmissionWithoutEvidenceRequestUseCase;

        public DocumentSubmissionsController(
            ICreateAuditUseCase createAuditUseCase,
            IUpdateDocumentSubmissionStateUseCase updateDocumentSubmissionStateUseCase,
            IUpdateDocumentSubmissionVisibiltyUseCase updateDocumentSubmissionVisibilityUseCase,
            IFindDocumentSubmissionByIdUseCase findDocumentSubmissionByIdUseCase,
            IFindDocumentSubmissionsByResidentIdUseCase findDocumentSubmissionsByResidentIdUseCase,
            ICreateDocumentSubmissionWithoutEvidenceRequestUseCase createDocumentSubmissionWithoutEvidenceRequestUseCase
        ) : base(createAuditUseCase)
        {
            _updateDocumentSubmissionStateUseCase = updateDocumentSubmissionStateUseCase;
            _updateDocumentSubmissionVisibilityUseCase = updateDocumentSubmissionVisibilityUseCase;
            _findDocumentSubmissionByIdUseCase = findDocumentSubmissionByIdUseCase;
            _findDocumentSubmissionsByResidentIdUseCase = findDocumentSubmissionsByResidentIdUseCase;
            _createDocumentSubmissionWithoutEvidenceRequestUseCase = createDocumentSubmissionWithoutEvidenceRequestUseCase;
        }

        /// <summary>
        /// Updates the state of a document submission
        /// </summary>
        /// <response code="200">Updated</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="404">Document submission cannot be found</response>
        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDocumentSubmissionState([FromRoute][Required] Guid id, [FromBody] DocumentSubmissionUpdateRequest request)
        {
            try
            {
                var result = await _updateDocumentSubmissionStateUseCase.ExecuteAsync(id, request).ConfigureAwait(true);
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
        /// Updates the visibility of a document submission
        /// </summary>
        /// <response code="200">Updated</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="404">Document submission cannot be found</response>
        [HttpPatch]
        [Route("{id}/visibility")]
        public IActionResult UpdateDocumentSubmissionVisibility([FromRoute][Required] Guid id, [FromBody] DocumentSubmissionVisibilityUpdateRequest request)
        {
            try
            {
                _updateDocumentSubmissionVisibilityUseCase.ExecuteAsync(id, request);
                return Ok();
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
        /// Finds a document submission by id
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="404">Document submission cannot be found</response>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> FindDocumentSubmission([FromRoute][Required] Guid id)
        {
            try
            {
                var result = await _findDocumentSubmissionByIdUseCase.ExecuteAsync(id).ConfigureAwait(true);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Find all document submissions by resident id
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="400">Search query is invalid</response>
        [HttpGet]
        public async Task<IActionResult> FindDocumentSubmissionsByResidentId([FromQuery][Required] DocumentSubmissionSearchQuery request)
        {
            try
            {
                var result = await _findDocumentSubmissionsByResidentIdUseCase.ExecuteAsync(request).ConfigureAwait(true);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new document submission without an evidence request
        /// </summary>
        /// <response code="201">Saved</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="401">Request lacks valid API token</response>
        [HttpPost]
        public async Task<IActionResult> CreateDocumentSubmissionWithoutEvidenceRequest([FromBody][Required] DocumentSubmissionWithoutEvidenceRequestRequest request)
        {
            try
            {
                var result = await _createDocumentSubmissionWithoutEvidenceRequestUseCase.ExecuteAsync(request);
                return Created(new Uri($"/document_submissions", UriKind.Relative), result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
