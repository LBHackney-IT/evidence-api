using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class FindResidentByIDUseCase : IFindResidentByIDUseCase
    {
        private readonly IResidentsGateway _residentsGateway;

        public FindResidentByIDUseCase(IResidentsGateway residentsGateway)
        {
            _residentsGateway = residentsGateway;
        }

        public ResidentResponse Execute(Guid id)
        {
            var found = _residentsGateway.FindResident(id);

            if (found == null) throw new NotFoundException($"Could not find resident with id {id}");

            return found.ToResponse();
        }
    }
}
