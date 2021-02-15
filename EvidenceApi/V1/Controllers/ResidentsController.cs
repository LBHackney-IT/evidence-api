using System;
using System.ComponentModel.DataAnnotations;
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
        private readonly IFindResidentByIDUseCase _findByIdUseCase;
        private readonly IFindResidentsBySearchQueryUseCase _findResidentsBySearchQueryUseCase;

        public ResidentsController(IFindResidentByIDUseCase findByIdUseCase, IFindResidentsBySearchQueryUseCase findResidentsBySearchQueryUseCase)
        {
            _findByIdUseCase = findByIdUseCase;
            _findResidentsBySearchQueryUseCase = findResidentsBySearchQueryUseCase;
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
        [Route("search/{searchQuery}")]
        public IActionResult SearchResidents([FromRoute][Required] string searchQuery)
        {
            var result = _findResidentsBySearchQueryUseCase.Execute(searchQuery);

            return Ok(result);
        }
    }
}
