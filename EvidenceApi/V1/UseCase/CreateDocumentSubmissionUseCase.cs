using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using System.Threading.Tasks;

namespace EvidenceApi.V1.UseCase
{
    public class CreateDocumentSubmissionUseCase : ICreateDocumentSubmissionUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;

        public CreateDocumentSubmissionUseCase(IEvidenceGateway evidenceGateway, IDocumentsApiGateway documentsApiGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentsApiGateway = documentsApiGateway;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid evidenceRequestId, DocumentSubmissionRequest request)
        {
            ValidateRequest(request);

            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            var claimRequest = BuildClaimRequest(evidenceRequest);
            var claim = await _documentsApiGateway.CreateClaim(claimRequest).ConfigureAwait(true);

            var documentSubmission = BuildDocumentSubmission(evidenceRequest, request, claim);
            var createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);

            var createdS3UploadPolicy = await _documentsApiGateway.CreateUploadPolicy(claim.Document.Id).ConfigureAwait(true);

            return createdDocumentSubmission.ToResponse(createdS3UploadPolicy);
        }

        private static ClaimRequest BuildClaimRequest(EvidenceRequest evidenceRequest)
        {
            var claimRequest = new ClaimRequest()
            {
                ServiceAreaCreatedBy = evidenceRequest.ServiceRequestedBy,
                UserCreatedBy = evidenceRequest.UserRequestedBy,
                ApiCreatedBy = "evidence_api",
                RetentionExpiresAt = DateTime.Now.AddMonths(3)
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
                ClaimId = claim.Id.ToString()
            };
            return documentSubmission;
        }

        private static void ValidateRequest(DocumentSubmissionRequest request)
        {
            if (String.IsNullOrEmpty(request.DocumentType))
            {
                throw new BadRequestException("Document type is null or empty");
            }
        }
    }
}
