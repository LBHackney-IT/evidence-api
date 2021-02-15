using System.Collections.Generic;
using System.Linq;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class FindResidentsBySearchQueryUseCase : IFindResidentsBySearchQueryUseCase
    {
        private readonly IResidentsGateway _residentsGateway;

        public FindResidentsBySearchQueryUseCase(IResidentsGateway residentsGateway)
        {
            _residentsGateway = residentsGateway;
        }

        public List<ResidentResponse> Execute(string searchQuery)
        {
            var found = _residentsGateway.FindResidents(searchQuery);

            return found.Select(r => r.ToResponse()).ToList();
        }
    }
}
