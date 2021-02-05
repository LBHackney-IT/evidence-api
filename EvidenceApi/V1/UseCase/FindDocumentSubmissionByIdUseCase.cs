using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase
{
    public class FindDocumentSubmissionByIdUseCase : IFindDocumentSubmissionByIdUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;

        public FindDocumentSubmissionByIdUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
        }

        public DocumentSubmissionResponse Execute(Guid id)
        {
            var found = _evidenceGateway.FindDocumentSubmission(id);

            if (found == null)
            {
                throw new NotFoundException($"Cannot find document submission with ID: {id}");
            }

            var documentType = FindDocumentType(found.DocumentTypeId);
            return found.ToResponse(documentType);
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }
    }
}
