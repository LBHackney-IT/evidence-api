using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.UseCase
{
    public class FindEvidenceRequestUseCase : IFindEvidenceRequestUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;

        public FindEvidenceRequestUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IResidentsGateway residentsGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
        }
        public EvidenceRequestResponse Execute(Guid id)
        {
            if (id == null)
            {
                throw new BadRequestException("Id is null");
            }

            var found = _evidenceGateway.FindEvidenceRequest(id);

            if (found == null)
            {
                throw new NotFoundException("Cannot retrieve evidence request");
            }

            var resident = _residentsGateway.FindResident(found.ResidentId);
            var documentTypes = found.DocumentTypeIds.ConvertAll(FindDocumentType);
            return found.ToResponse(resident, documentTypes);
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }
    }
}