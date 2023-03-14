using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Text.Json;

namespace EvidenceApi.V1.UseCase
{
    public class CreateMergedResidentUseCase : ICreateMergedResidentUseCase
    {
        private readonly IResidentsGateway _residentsGateway;
        private readonly ILogger<CreateResidentUseCase> _logger;

        public CreateMergedResidentUseCase(IResidentsGateway residentsGateway, ILogger<CreateResidentUseCase> logger)
        {
            _residentsGateway = residentsGateway;
            _logger = logger;
        }

        public ResidentResponse Execute(ResidentRequest request)
        {
            var validation = new ResidentRequestValidator().Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation.Errors.First().ToString());
            }

            var resident = new Resident()
            {
                Email = request.Email,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber
            };

            var createdResident = _residentsGateway.CreateResident(resident);

            _residentsGateway.AddResidentGroupId(createdResident.Id, request.Team, request.GroupId);

            return createdResident.ToResponse();
        }

    }
}
