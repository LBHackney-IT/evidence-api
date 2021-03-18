using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using System.Collections.Generic;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.UseCase
{
    public class FindEvidenceRequestsUseCase : IFindEvidenceRequestsUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;

        public FindEvidenceRequestsUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IResidentsGateway residentsGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
        }

        public List<EvidenceRequestResponse> Execute(EvidenceRequestsSearchQuery request)
        {
            var found = _evidenceGateway.GetEvidenceRequests(request);

            if (String.IsNullOrEmpty(request.ServiceRequestedBy))
            {
                throw new BadRequestException("Service requested by is null or empty");
            }

            return found.ConvertAll<EvidenceRequestResponse>(er =>
            {
                var resident = _residentsGateway.FindResident(er.ResidentId);
                var documentTypes = er.DocumentTypes.ConvertAll<DocumentType>(dt => _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentId(request.ServiceRequestedBy,dt));
                return er.ToResponse(resident, documentTypes);
            });
        }
    }
}
