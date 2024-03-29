using System;
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
        private readonly IResidentsGateway _residentsGateway;
        private readonly IUpdateEvidenceRequestStateUseCase _updateEvidenceRequestStateUseCase;

        public CreateDocumentSubmissionUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentsApiGateway documentsApiGateway,
            IDocumentTypeGateway documentTypeGateway,
            IResidentsGateway residentsGateway,
            IUpdateEvidenceRequestStateUseCase updateEvidenceRequestStateUseCase)
        {
            _evidenceGateway = evidenceGateway;
            _documentsApiGateway = documentsApiGateway;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
            _updateEvidenceRequestStateUseCase = updateEvidenceRequestStateUseCase;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid evidenceRequestId, DocumentSubmissionRequest request)
        {
            ValidateRequest(request);

            var evidenceRequest = _evidenceGateway.FindEvidenceRequestWithDocumentSubmissions(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            // Commenting out for now as we have partial uploads implemented.
            // if (evidenceRequest.DocumentSubmissions != null && evidenceRequest.DocumentSubmissions.Any(d =>
            //         (d.State == SubmissionState.Approved || d.State == SubmissionState.Uploaded) &&
            //         d.DocumentTypeId == request.DocumentType))
            // {
            //     throw new BadRequestException(ThrowsBadRequestIfActiveDocumentSubmissionAlreadyExists
            //         $"An active document submission for document type ${request.DocumentType} already exists");
            // }

            Claim claim;
            S3UploadPolicy createdS3UploadPolicy;

            var groupId = _residentsGateway.FindGroupIdByResidentIdAndTeam(evidenceRequest.ResidentId, evidenceRequest.Team);
            if (groupId == null)
            {
                groupId = _residentsGateway.AddResidentGroupId(evidenceRequest.ResidentId, evidenceRequest.Team, null);
            }
            try
            {
                var claimRequest = BuildClaimRequest(evidenceRequest, (Guid) groupId);
                claim = await _documentsApiGateway.CreateClaim(claimRequest);
                createdS3UploadPolicy = await _documentsApiGateway.CreateUploadPolicy(claim.Document.Id);
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException($"Issue with DocumentsApi so cannot create DocumentSubmission: {ex.Message}");
            }

            var documentSubmission = BuildDocumentSubmission(evidenceRequest, request, claim);
            var createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(evidenceRequest.Team, documentSubmission.DocumentTypeId);

            if (createdDocumentSubmission.EvidenceRequestId != null)
            {
                _updateEvidenceRequestStateUseCase.Execute((Guid) createdDocumentSubmission.EvidenceRequestId);
            }

            return createdDocumentSubmission.ToResponse(documentType, documentSubmission.EvidenceRequestId, null, createdS3UploadPolicy, claim);
        }

        private static ClaimRequest BuildClaimRequest(EvidenceRequest evidenceRequest, Guid groupId)
        {
            var claimRequest = new ClaimRequest()
            {
                ServiceAreaCreatedBy = evidenceRequest.Team,
                UserCreatedBy = evidenceRequest.UserRequestedBy,
                ApiCreatedBy = "evidence_api",
                RetentionExpiresAt = DateTime.UtcNow.AddMonths(3).Date,
                ValidUntil = DateTime.UtcNow.AddMonths(3).Date,
                GroupId = groupId
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
                EvidenceRequestId = evidenceRequest.Id,
                DocumentTypeId = request.DocumentType,
                ClaimId = claim.Id.ToString(),
                State = SubmissionState.Uploaded,
                Team = evidenceRequest.Team,
                ResidentId = evidenceRequest.ResidentId
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
