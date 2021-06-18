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
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase
{
    public class CreateDocumentSubmissionUseCase : ICreateDocumentSubmissionUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly ILogger<CreateDocumentSubmissionUseCase> _logger;

        public CreateDocumentSubmissionUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentsApiGateway documentsApiGateway,
            IDocumentTypeGateway documentTypeGateway,
            ILogger<CreateDocumentSubmissionUseCase> logger)
        {
            _evidenceGateway = evidenceGateway;
            _documentsApiGateway = documentsApiGateway;
            _documentTypeGateway = documentTypeGateway;
            _logger = logger;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid evidenceRequestId, DocumentSubmissionRequest request)
        {
            //ValidateRequest(request);

            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            if (evidenceRequest.DocumentSubmissions != null && evidenceRequest.DocumentSubmissions.Any(d =>
                (d.State == SubmissionState.Approved || d.State == SubmissionState.Uploaded) && d.DocumentTypeId == "test"))
            {
                throw new BadRequestException($"An active document submission for document type ${"test"} already exists");
            }

            try
            {
                var claimRequest = BuildClaimRequest(evidenceRequest);
                var claim = await _documentsApiGateway.CreateClaim(claimRequest).ConfigureAwait(true);
                var uploadStatus = await _documentsApiGateway.UploadDocument(claim.Document.Id, request).ConfigureAwait(true);
                _logger.LogDebug("Document uploaded status for ClaimID: {0}, DocumentId: {1}, Status: {2}", claim.Id, claim.Document.Id, uploadStatus);

                var documentSubmission = BuildDocumentSubmission(evidenceRequest, claim);
                var createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);

                var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(evidenceRequest.Team, documentSubmission.DocumentTypeId);

                return createdDocumentSubmission.ToResponse(documentType);
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
            //DocumentSubmissionRequest request,
            Claim claim
        )
        {
            var documentSubmission = new DocumentSubmission()
            {
                EvidenceRequest = evidenceRequest,
                DocumentTypeId = "test",
                ClaimId = claim.Id.ToString(),
                State = SubmissionState.Uploaded
            };
            return documentSubmission;
        }

        private static void ValidateRequest()
        {
            if (String.IsNullOrEmpty("test"))
            {
                throw new BadRequestException("Document type is null or empty");
            }
        }
    }
}
