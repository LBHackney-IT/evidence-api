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
using EvidenceApi.V1.Validators;
using System.Linq;

namespace EvidenceApi.V1.UseCase
{
    public class CreateDocumentSubmissionWithoutEvidenceRequestUseCase : ICreateDocumentSubmissionWithoutEvidenceRequestUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;

        public CreateDocumentSubmissionWithoutEvidenceRequestUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentsApiGateway documentsApiGateway,
            IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentsApiGateway = documentsApiGateway;
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
        }

        public async Task<DocumentSubmissionWithoutEvidenceRequestResponse> ExecuteAsync(DocumentSubmissionWithoutEvidenceRequestRequest request)
        {
            var validation = new DocumentSubmissionWithoutEvidenceRequestRequestValidator().Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation.Errors.First().ToString());
            }

            Claim claim;
            S3UploadPolicy createdS3UploadPolicy;

            try
            {
                var claimRequest = BuildClaimRequest(request);
                claim = await _documentsApiGateway.CreateClaim(claimRequest);
                createdS3UploadPolicy = await _documentsApiGateway.CreateUploadPolicy(claim.Document.Id);
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException($"Issue with DocumentsApi so cannot create DocumentSubmission: {ex.Message}");
            }

            var documentSubmission = BuildDocumentSubmission(request, claim);
            var createdDocumentSubmission = new DocumentSubmission();
            try
            {
                createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            }
            catch (Exception)
            {
                throw new BadRequestException($"A resident with ID {request.ResidentId} does not exist.");
            }

            var staffSelectedDocumentType = _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(request.Team, documentSubmission.StaffSelectedDocumentTypeId);

            return createdDocumentSubmission.ToResponse(staffSelectedDocumentType, createdS3UploadPolicy, claim);
        }

        private static ClaimRequest BuildClaimRequest(DocumentSubmissionWithoutEvidenceRequestRequest request)
        {
            var claimRequest = new ClaimRequest()
            {
                ServiceAreaCreatedBy = request.Team,
                UserCreatedBy = request.UserCreatedBy,
                ApiCreatedBy = "evidence_api",
                RetentionExpiresAt = DateTime.UtcNow.AddMonths(3).Date,
                ValidUntil = DateTime.UtcNow.AddMonths(3).Date
            };
            return claimRequest;
        }

        private static DocumentSubmission BuildDocumentSubmission(
            DocumentSubmissionWithoutEvidenceRequestRequest request,
            Claim claim
        )
        {
            var currentDateTime = DateTime.UtcNow;
            var documentSubmission = new DocumentSubmission()
            {
                CreatedAt = currentDateTime,
                ClaimId = claim.Id.ToString(),
                State = SubmissionState.Approved,
                AcceptedAt = currentDateTime,
                Team = request.Team,
                ResidentId = request.ResidentId,
                StaffSelectedDocumentTypeId = request.StaffSelectedDocumentTypeId
            };
            return documentSubmission;
        }
    }
}
