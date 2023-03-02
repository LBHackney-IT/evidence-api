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
            FindResidentByGroupId(request, residents);
            FindByResidentReferenceId(request, residents);

            var uniqueResidents = residents.GroupBy(x => x.Id).Select(y => y.First());

            return uniqueResidents.ToList();
        }

        private void FindByResidentDetails(ResidentSearchQuery request, ICollection<ResidentResponse> residents)
        {
            var residentsForSearchQuery = _residentsGateway.FindResidents(request.SearchQuery);
            foreach (var resident in residentsForSearchQuery)
            {
                var evidenceRequestsForResident = _evidenceGateway.FindEvidenceRequestsByResidentId(resident.Id);
                var residentReferenceId = "";
                foreach (var evidenceRequest in evidenceRequestsForResident)
                {
                    if (evidenceRequest.Team.Equals(request.Team))
                    {
                        residentReferenceId = evidenceRequest.ResidentReferenceId;
                    }
                }
                residents.Add(resident.ToResponse(residentReferenceId));
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


        private void FindResidentByGroupId(ResidentSearchQuery request,
            ICollection<ResidentResponse> residents)
        {
            var found = _residentsGateway.FindResidentByGroupId(request);
            if (found != null)
            {
                residents.Add(found.ToResponse());
            }
        }

    }
}
