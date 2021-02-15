using System;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using System.Threading.Tasks;

namespace EvidenceApi.V1.UseCase
{
    public class FindDocumentSubmissionByIdUseCase : IFindDocumentSubmissionByIdUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private IDocumentsApiGateway _documentsApiGateway;

        public FindDocumentSubmissionByIdUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentTypeGateway documentTypeGateway,
            IDocumentsApiGateway documentsApiGateway
        )
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _documentsApiGateway = documentsApiGateway;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid id)
        {
            var found = _evidenceGateway.FindDocumentSubmission(id);

            if (found == null)
            {
                throw new NotFoundException($"Cannot find document submission with ID: {id}");
            }

            var documentType = FindDocumentType(found.DocumentTypeId);
            var claim = await _documentsApiGateway.GetClaimById(found.ClaimId).ConfigureAwait(true);
            return found.ToResponse(documentType, null, claim.Document);
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }
    }
}
