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
        private readonly IAmendResidentGroupIdUseCase _amendResidentGroupIdUseCase;

        public ResidentsController(
            IFindResidentByIdUseCase findByIdUseCase,
            IFindResidentsBySearchQueryUseCase findResidentsBySearchQueryUseCase,
            ICreateAuditUseCase createAuditUseCase,
            ICreateResidentUseCase createResidentUseCase,
            IAmendResidentGroupIdUseCase amendResidentGroupIdUseCase
        ) : base(createAuditUseCase)
        {
            _findByIdUseCase = findByIdUseCase;
            _findResidentsBySearchQueryUseCase = findResidentsBySearchQueryUseCase;
            _createResidentUseCase = createResidentUseCase;
            _amendResidentGroupIdUseCase = amendResidentGroupIdUseCase;
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
    }
}
