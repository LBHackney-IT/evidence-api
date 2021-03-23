using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase
{
    public class FindEvidenceRequestByIdUseCase : IFindEvidenceRequestByIdUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;

        public FindEvidenceRequestByIdUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IResidentsGateway residentsGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
        }

        public EvidenceRequestResponse Execute(Guid id)
        {
            var found = _evidenceGateway.FindEvidenceRequest(id);

            if (found == null)
            {
                throw new NotFoundException("Cannot retrieve evidence request");
            }

            var resident = _residentsGateway.FindResident(found.ResidentId);
            var teamName = found.ServiceRequestedBy;
            var documentTypes = found.DocumentTypes.ConvertAll((documentTypeId) => FindDocumentType(teamName, documentTypeId));
            return found.ToResponse(resident, documentTypes);
        }

        private DocumentType FindDocumentType(string teamName, string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentId(teamName, documentTypeId);
        }
    }
}
