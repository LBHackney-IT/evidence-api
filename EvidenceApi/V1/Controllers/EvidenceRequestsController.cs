using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

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

        public EvidenceRequestsController(
            ICreateEvidenceRequestUseCase creator,
            ICreateDocumentSubmissionUseCase createDocumentSubmission,
            IFindEvidenceRequestByIdUseCase evidenceRequestUseCase,
            IFindEvidenceRequestsUseCase getEvidenceRequestsUseCase,
            ICreateAuditUseCase createAuditUseCase
        ) : base(createAuditUseCase)
        {
            _creator = creator;
            _createDocumentSubmission = createDocumentSubmission;
            _evidenceRequestUseCase = evidenceRequestUseCase;
            _getEvidenceRequestsUseCase = getEvidenceRequestsUseCase;
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
        [RequestSizeLimit(5000000)]
        [Route("{evidenceRequestId}/document_submissions")]
        public IActionResult CreateDocumentSubmission([FromForm] DocumentSubmissionRequest request)
        {
            // var httprequest = HttpContext.Request;
            // var formCollection = httprequest.Form;
            // Console.WriteLine("file {0}", data);
            Console.WriteLine("file {0}", request);
            // if (request.Form
            // {
            //     result = Request.CreateResponse(HttpStatusCode.BadRequest);
            // }
            // try
            // {
            //     Console.WriteLine("request {0}", evidenceRequestId);
            //     Console.WriteLine("file {0}", request);
            //     var result = await _createDocumentSubmission.ExecuteAsync(evidenceRequestId, request).ConfigureAwait(true);
            //     return Created(
            //         new Uri($"/evidence_requests/{evidenceRequestId}/document_submissions", UriKind.Relative), result);
            // }
            // catch (BadRequestException ex)
            // {
            //     return BadRequest(ex.Message);
            // }
            // catch (NotFoundException ex)
            // {
            //     return NotFound(ex.Message);
            // }
            return BadRequest("asdas");
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
