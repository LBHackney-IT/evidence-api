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
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;
        private IDocumentsApiGateway _documentsApiGateway;

        public FindDocumentSubmissionByIdUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentTypeGateway documentTypeGateway,
            IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway,
            IDocumentsApiGateway documentsApiGateway
        )
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
            _documentsApiGateway = documentsApiGateway;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid id)
        {
            var found = _evidenceGateway.FindDocumentSubmission(id);

            if (found == null)
            {
                throw new NotFoundException($"Cannot find document submission with ID: {id}");
            }

            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(found.EvidenceRequestId);
            var documentType = FindDocumentType(evidenceRequest.ServiceRequestedBy, found.DocumentTypeId);
            var staffSelectedDocumentType = FindStaffSelectedDocumentType(evidenceRequest.ServiceRequestedBy,
                found.StaffSelectedDocumentTypeId);
            var claim = await _documentsApiGateway.GetClaimById(found.ClaimId).ConfigureAwait(true);
            return found.ToResponse(documentType, staffSelectedDocumentType, null, claim.Document);
        }

        private DocumentType FindDocumentType(string teamName, string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, documentTypeId);
        }

        private DocumentType FindStaffSelectedDocumentType(string teamName, string staffSelectedDocumentTypeId)
        {
            return _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, staffSelectedDocumentTypeId);
        }
    }
}
