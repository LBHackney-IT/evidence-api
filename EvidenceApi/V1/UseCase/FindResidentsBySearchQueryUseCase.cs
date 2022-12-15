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
            var residents = new List<ResidentResponse>();
            FindByResidentDetails(request, residents);
            FindResidentsWithNoAttachedRequests(request, residents);
            FindByResidentReferenceId(request, residents);
            return residents;
        }

        private void FindByResidentDetails(ResidentSearchQuery request, ICollection<ResidentResponse> residents)
        {
            var residentsForSearchQuery = _residentsGateway.FindResidents(request.SearchQuery);

            foreach (var resident in residentsForSearchQuery)
            {
                var evidenceRequestsForResident = _evidenceGateway.FindEvidenceRequestsByResidentId(resident.Id);
                foreach (var evidenceRequest in evidenceRequestsForResident)
                {
                    if (evidenceRequest.Team.Equals(request.Team))
                    {
                        residents.Add(resident.ToResponse(evidenceRequest.ResidentReferenceId));
                        // if any of the evidence requests were created by Team from the query then this resident should be returned
                        // and we can then break out of the loop early.
                        break;
                    }
                }
            }
        }

        private void FindByResidentReferenceId(ResidentSearchQuery request, ICollection<ResidentResponse> residents)
        {
            var evidenceRequestsForTeamAndSearchQuery = _evidenceGateway.GetEvidenceRequests(request);
            if (evidenceRequestsForTeamAndSearchQuery.Count > 0)
            {
                var evidenceRequest = evidenceRequestsForTeamAndSearchQuery.First();
                var residentForTeamAndSearchQuery = _residentsGateway.FindResident(evidenceRequest.ResidentId);
                residents.Add(residentForTeamAndSearchQuery.ToResponse(evidenceRequest.ResidentReferenceId));
            }
        }

        private void FindResidentsWithNoAttachedRequests(ResidentSearchQuery request,
            ICollection<ResidentResponse> residents)
        {
            var residentsForSearchQuery = _residentsGateway.FindResidents(request.SearchQuery);


            foreach (var resident in residentsForSearchQuery)
            {
                //should the resident object have a team property? Otherwise we will return residents created by all teams...
                //or is this what we want?  I think it's what we want.
                residents.Add(resident.ToResponse());
            };
        }
    }
}
