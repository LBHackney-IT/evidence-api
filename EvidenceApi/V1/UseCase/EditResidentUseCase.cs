using System;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Boundary.Response.Exceptions;

namespace EvidenceApi.V1.UseCase
{
    public class EditResidentUseCase : IEditResidentUseCase
    {
        private readonly IResidentsGateway _residentsGateway;

        public EditResidentUseCase(IResidentsGateway residentsGateway)
        {
            _residentsGateway = residentsGateway;
        }

        public ResidentResponse Execute(Guid residentId, EditResidentRequest request)
        {
            if (request == null)
            {
                throw new BadRequestException("The request body is empty");
            }

            var resident = _residentsGateway.FindResident(residentId);
            if (resident == null)
            {
                throw new NotFoundException($"The resident with id {residentId} could not be found");
            }

            if (!String.IsNullOrEmpty(request.Name))
            {
                resident.Name = request.Name;
            }

            if (!String.IsNullOrEmpty(request.Email))
            {
                resident.Email = request.Email;
            }

            if (!String.IsNullOrEmpty(request.PhoneNumber))
            {
                resident.PhoneNumber = request.PhoneNumber;
            }
            var editedResident = _residentsGateway.CreateResident(resident);
            return editedResident.ToResponse();
        }
    }
}
