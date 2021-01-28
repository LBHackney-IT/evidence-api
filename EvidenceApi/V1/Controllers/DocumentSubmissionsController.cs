using System;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/document_submissions")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class DocumentSubmissionsController : BaseController
    {
        private readonly IUpdateDocumentSubmissionStateUseCase _updateDocumentSubmissionStateUseCase;

        public DocumentSubmissionsController(IUpdateDocumentSubmissionStateUseCase updateDocumentSubmissionStateUseCase)
        {
            _updateDocumentSubmissionStateUseCase = updateDocumentSubmissionStateUseCase;
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
    }
}
