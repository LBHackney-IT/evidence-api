using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class GetStaffSelectedDocumentTypesByTeamNameUseCase : IGetStaffSelectedDocumentTypesByTeamNameUseCase
    {
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;

        public GetStaffSelectedDocumentTypesByTeamNameUseCase(IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway)
        {
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
        }

        public List<DocumentType> Execute(string team, bool? enabled)
        {
            var result = _staffSelectedDocumentTypeGateway.GetDocumentTypesByTeamName(team);

            if (result.Count > 0 && enabled.HasValue)
            {
                var resultEnabled = result.FindAll(dt => dt.Enabled == enabled);
                return resultEnabled;
            }

            if (result.Count > 0)
            {
                return result;
            }

            throw new NotFoundException($"No staff-selected document types were found for team with name: {team}");
        }

    }
}
