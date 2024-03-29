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

            var documentType = FindDocumentType(found.Team, found.DocumentTypeId);
            var staffSelectedDocumentType = FindStaffSelectedDocumentType(found.Team,
                found.StaffSelectedDocumentTypeId);
            try
            {
                var claim = await _documentsApiGateway.GetClaimById(found.ClaimId).ConfigureAwait(true);
                return found.ToResponse(documentType, found.EvidenceRequestId, staffSelectedDocumentType, null, claim);
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException($"Issue with DocumentsApi so cannot return DocumentSubmissionResponse: {ex.Message}");
            }
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
