using System.Collections.Generic;
using System.Linq;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class FindResidentsBySearchQueryUseCase : IFindResidentsBySearchQueryUseCase
    {
        private readonly IResidentsGateway _residentsGateway;
        private readonly IEvidenceGateway _evidenceGateway;

        public FindResidentsBySearchQueryUseCase(IResidentsGateway residentsGateway, IEvidenceGateway evidenceGateway)
        {
            _residentsGateway = residentsGateway;
            _evidenceGateway = evidenceGateway;
        }

        public List<ResidentResponse> Execute(ResidentSearchQuery request)
        {
            var results = new List<ResidentResponse>();
            findByResidentDetails(request, results);
            findByResidentReferenceId(request, results);
            return results;
        }

        private void findByResidentDetails(ResidentSearchQuery request, ICollection<ResidentResponse> results)
        {
            var residentSearchByNameEmailPhoneNumber = _residentsGateway.FindResidents(request.SearchQuery);

            foreach (var resident in residentSearchByNameEmailPhoneNumber)
            {
                var evidenceRequestsForResident = _evidenceGateway.FindEvidenceRequestsByResidentId(resident.Id);
                foreach (var evidenceRequest in evidenceRequestsForResident)
                {
                    if (evidenceRequest.ServiceRequestedBy.Equals(request.ServiceRequestedBy))
                    {
                        results.Add(resident.ToResponse(evidenceRequest.ResidentReferenceId));
                        // if any of the evidence requests were created by ServiceRequestedBy from the query then this resident should be returned
                        // we can then break out of the loop early
                        break;
                    }
                }
            }
        }

        private void findByResidentReferenceId(ResidentSearchQuery request, ICollection<ResidentResponse> results)
        {
            var evidenceRequestsByTeamAndResidentReferenceId = _evidenceGateway.GetEvidenceRequests(request);
            if (evidenceRequestsByTeamAndResidentReferenceId.Count > 0)
            {
                var evidenceRequest = evidenceRequestsByTeamAndResidentReferenceId.First();
                var residentByTeamAndResidentReferenceId = _residentsGateway.FindResident(evidenceRequest.ResidentId);
                results.Add(residentByTeamAndResidentReferenceId.ToResponse(evidenceRequest.ResidentReferenceId));
            }
        }
    }
}
