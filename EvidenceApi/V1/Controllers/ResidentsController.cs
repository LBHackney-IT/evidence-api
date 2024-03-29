using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/residents")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ResidentsController : BaseController
    {
        private readonly IFindResidentByIdUseCase _findByIdUseCase;
        private readonly IFindResidentsBySearchQueryUseCase _findResidentsBySearchQueryUseCase;
        private readonly ICreateResidentUseCase _createResidentUseCase;
        private readonly IAmendClaimsGroupIdUseCase _amendResidentGroupIdUseCase;
        private readonly IMergeAndLinkResidentsUseCase _mergeAndLinkResidentsUseCase;
        private readonly IEditResidentUseCase _editResidentUseCase;

        public ResidentsController(
            IFindResidentByIdUseCase findByIdUseCase,
            IFindResidentsBySearchQueryUseCase findResidentsBySearchQueryUseCase,
            ICreateAuditUseCase createAuditUseCase,
            ICreateResidentUseCase createResidentUseCase,
            IAmendClaimsGroupIdUseCase amendResidentGroupIdUseCase,
            IMergeAndLinkResidentsUseCase mergeAndLinkResidentsUseCase,
            IEditResidentUseCase editResidentUseCase
        ) : base(createAuditUseCase)
        {
            _findByIdUseCase = findByIdUseCase;
            _findResidentsBySearchQueryUseCase = findResidentsBySearchQueryUseCase;
            _createResidentUseCase = createResidentUseCase;
            _amendResidentGroupIdUseCase = amendResidentGroupIdUseCase;
            _mergeAndLinkResidentsUseCase = mergeAndLinkResidentsUseCase;
            _editResidentUseCase = editResidentUseCase;
        }

        /// <summary>
        /// Finds resident
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="401">Request lacks valid API token</response>
        /// <response code="404">Resource not found</response>
        [HttpGet]
        [Route("{id}")]
        public IActionResult FindResident([FromRoute][Required] Guid id)
        {
            try
            {
                var result = _findByIdUseCase.Execute(id);
                return Ok(result);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Search for residents
        /// </summary>
        /// <response code="200">Found</response>
        /// <response code="401">Request lacks valid API token</response>
        [HttpGet]
        [Route("search")]
        public IActionResult SearchResidents([FromQuery][Required] ResidentSearchQuery request)
        {
            var result = _findResidentsBySearchQueryUseCase.Execute(request);

            return Ok(result);
        }

        /// <summary>
        /// Create a resident
        /// </summary>
        /// <response code="201">Created</response>
        /// <response code="401">Request lacks valid API token</response>
        /// <response code="400">Bad request</response>
        [HttpPost]
        public IActionResult CreateResident(ResidentRequest request)
        {
            try
            {
                var result = _createResidentUseCase.Execute(request);
                return Created(new Uri($"/residents", UriKind.Relative), result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("update-group-id")]
        public async Task<IActionResult> AmendResidentGroupId([FromBody] ResidentGroupIdRequest request)
        {
            try
            {
                var result = await _amendResidentGroupIdUseCase.Execute(request);
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

        [HttpPost]
        [Route("merge-and-link")]
        public async Task<IActionResult> MergeAndLinkResidents([FromBody] MergeAndLinkResidentsRequest request)
        {
            try
            {
                var result = await _mergeAndLinkResidentsUseCase.ExecuteAsync(request);
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

        [HttpPatch]
        [Route("{residentId}")]
        public IActionResult EditResident([FromRoute][Required] Guid residentId, [FromBody] EditResidentRequest request)
        {
            try
            {
                var result = _editResidentUseCase.Execute(residentId, request);
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
