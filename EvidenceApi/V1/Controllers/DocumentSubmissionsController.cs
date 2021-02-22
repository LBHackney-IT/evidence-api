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
        private readonly IFindDocumentSubmissionByIdUseCase _findDocumentSubmissionByIdUseCase;
        private readonly IFindDocumentSubmissionsByResidentIdUseCase _findDocumentSubmissionsByResidentIdUseCase;

        public DocumentSubmissionsController(
            IUpdateDocumentSubmissionStateUseCase updateDocumentSubmissionStateUseCase,
            IFindDocumentSubmissionByIdUseCase findDocumentSubmissionByIdUseCase,
            IFindDocumentSubmissionsByResidentIdUseCase findDocumentSubmissionsByResidentIdUseCase
        )
        {
            _updateDocumentSubmissionStateUseCase = updateDocumentSubmissionStateUseCase;
            _findDocumentSubmissionByIdUseCase = findDocumentSubmissionByIdUseCase;
            _findDocumentSubmissionsByResidentIdUseCase = findDocumentSubmissionsByResidentIdUseCase;
        }

        /// <summary>
        /// Updates the state of a document submission
        /// </summary>
        /// <response code="200">Updated</response>
        /// <response code="400">Request contains invalid parameters</response>
        /// <response code="404">Document submission cannot be found</response>
        [HttpPatch]
        [Route("{id}")]
        public IActionResult UpdateDocumentSubmissionState([FromRoute][Required] Guid id, [FromBody] DocumentSubmissionRequest request)
        {
            try
            {
                var result = _updateDocumentSubmissionStateUseCase.Execute(id, request);
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
        }

        /// <summary>
        /// Find all document submissions by resident id
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="404">Evidence request cannot be found</response>
        [HttpGet]
        public async Task<IActionResult> FindDocumentSubmissionsByResidentId([FromQuery] DocumentSubmissionSearchQuery request)
        {
            try
            {
                var result = await _findDocumentSubmissionsByResidentIdUseCase.ExecuteAsync(request).ConfigureAwait(true);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
