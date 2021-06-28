using System;
using System.Linq;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.UseCase
{
    public class CreateDocumentSubmissionUseCase : ICreateDocumentSubmissionUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;

        public CreateDocumentSubmissionUseCase(IEvidenceGateway evidenceGateway, IDocumentsApiGateway documentsApiGateway, IDocumentTypeGateway documentTypeGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentsApiGateway = documentsApiGateway;
            _documentTypeGateway = documentTypeGateway;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid evidenceRequestId, DocumentSubmissionRequest request)
        {
            ValidateRequest(request);

            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            if (evidenceRequest.DocumentSubmissions != null && evidenceRequest.DocumentSubmissions.Any(d =>
                (d.State == SubmissionState.Approved || d.State == SubmissionState.Uploaded) && d.DocumentTypeId == request.DocumentType))
            {
                throw new BadRequestException($"An active document submission for document type ${request.DocumentType} already exists");
            }

            try
            {
                var claimRequest = BuildClaimRequest(evidenceRequest);
                var claim = await _documentsApiGateway.CreateClaim(claimRequest).ConfigureAwait(true);
                await _documentsApiGateway.UploadDocument(claim.Document.Id, request).ConfigureAwait(true);

                var documentSubmission = BuildDocumentSubmission(evidenceRequest, request, claim);
                var createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);

                var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(evidenceRequest.Team, documentSubmission.DocumentTypeId);

                return createdDocumentSubmission.ToResponse(documentType, claim);
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException($"Issue with DocumentsApi so cannot create DocumentSubmission: {ex.Message}");
            }
        }

        private static ClaimRequest BuildClaimRequest(EvidenceRequest evidenceRequest)
        {
            var claimRequest = new ClaimRequest()
            {
                ServiceAreaCreatedBy = evidenceRequest.Team,
                UserCreatedBy = evidenceRequest.UserRequestedBy,
                ApiCreatedBy = "evidence_api",
                RetentionExpiresAt = DateTime.UtcNow.AddMonths(3).Date,
                ValidUntil = DateTime.UtcNow.AddMonths(3).Date
            };
            return claimRequest;
        }

        private static DocumentSubmission BuildDocumentSubmission(
            EvidenceRequest evidenceRequest,
            DocumentSubmissionRequest request,
            Claim claim
        )
        {
            var documentSubmission = new DocumentSubmission()
            {
                EvidenceRequest = evidenceRequest,
                DocumentTypeId = request.DocumentType,
                ClaimId = claim.Id.ToString(),
                State = SubmissionState.Uploaded
            };
            return documentSubmission;
        }

        private static void ValidateRequest(DocumentSubmissionRequest request)
        {
            if (String.IsNullOrEmpty(request.DocumentType))
            {
                throw new BadRequestException("Document type is null or empty");
            }
            if (String.IsNullOrEmpty(request.Base64Document))
            {
                throw new BadRequestException("Base 64 document is null or empty");
            }
        }
    }
}
