using System;
using System.Globalization;
using System.Threading.Tasks;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Domain.Enums;
using Notify.Exceptions;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCase : IUpdateDocumentSubmissionStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly INotifyGateway _notifyGateway;
        private readonly IResidentsGateway _residentsGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;
        private readonly IUpdateEvidenceRequestStateUseCase _updateEvidenceRequestStateUseCase;
        private readonly ILogger<UpdateDocumentSubmissionStateUseCase> _logger;

        public UpdateDocumentSubmissionStateUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentTypeGateway documentTypeGateway,
            INotifyGateway notifyGateway,
            IResidentsGateway residentsGateway,
            IDocumentsApiGateway documentsApiGateway,
            IStaffSelectedDocumentTypeGateway selectedDocumentTypeGateway,
            IUpdateEvidenceRequestStateUseCase updateEvidenceRequestStateUseCase,
            ILogger<UpdateDocumentSubmissionStateUseCase> logger
        )
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _notifyGateway = notifyGateway;
            _residentsGateway = residentsGateway;
            _documentsApiGateway = documentsApiGateway;
            _staffSelectedDocumentTypeGateway = selectedDocumentTypeGateway;
            _updateEvidenceRequestStateUseCase = updateEvidenceRequestStateUseCase;
            _logger = logger;
        }

        public async Task<DocumentSubmissionResponse> ExecuteAsync(Guid id, DocumentSubmissionUpdateRequest request)
        {
            var documentSubmission = _evidenceGateway.FindDocumentSubmission(id);

            if (documentSubmission == null)
            {
                throw new NotFoundException($"Cannot find document submission with id: {id}");
            }

            if (String.IsNullOrEmpty(request.State))
            {
                throw new BadRequestException("State in the request cannot be null");
            }

            SubmissionState state;
            if (!Enum.TryParse(request.State, true, out state))
            {
                throw new BadRequestException("This state is invalid");
            }

            if ((documentSubmission.State == SubmissionState.Approved ||
                documentSubmission.State == SubmissionState.Rejected) &&
                (request.State == "APPROVED" || request.State == "REJECTED"))
            {
                throw new BadRequestException($"Document has already been {documentSubmission.State.ToString().ToLower()}");
            }

            documentSubmission.State = state;
            documentSubmission.UserUpdatedBy = request.UserUpdatedBy;

            if (IsApprovalRequest(documentSubmission))
            {
                documentSubmission.AcceptedAt = DateTime.UtcNow;
            }

            if (IsRejectRequest(request, documentSubmission))
            {
                documentSubmission.RejectedAt = DateTime.UtcNow;
                NotifyResident(documentSubmission, request);
            }

            DocumentType staffSelectedDocumentType = null;
            if (RequestContainsStaffSelectedDocumentType(request))
            {
                staffSelectedDocumentType = GetStaffSelectedDocumentType(documentSubmission, request);
            }

            if (RequestContainsValidUntil(request))
            {
                await UpdateClaim(documentSubmission, request).ConfigureAwait(true);
            }

            _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            if (documentSubmission.EvidenceRequestId != null)
            {
                _updateEvidenceRequestStateUseCase.Execute((Guid) documentSubmission.EvidenceRequestId);
            }


            var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(documentSubmission.EvidenceRequest.Team, documentSubmission.DocumentTypeId);
            return documentSubmission.ToResponse(documentType, (Guid) documentSubmission.EvidenceRequestId, staffSelectedDocumentType);
        }

        private static bool IsApprovalRequest(DocumentSubmission documentSubmission)
        {
            return documentSubmission.State == SubmissionState.Approved;
        }

        private static bool IsRejectRequest(DocumentSubmissionUpdateRequest request, DocumentSubmission documentSubmission)
        {
            return !String.IsNullOrEmpty(request.RejectionReason) && documentSubmission.State == SubmissionState.Rejected;
        }

        private static bool RequestContainsValidUntil(DocumentSubmissionUpdateRequest request)
        {
            return !String.IsNullOrEmpty(request.ValidUntil);
        }

        private static bool RequestContainsStaffSelectedDocumentType(DocumentSubmissionUpdateRequest request)
        {
            return !String.IsNullOrEmpty(request.StaffSelectedDocumentTypeId);
        }

        private async Task<Claim> UpdateClaim(DocumentSubmission documentSubmission, DocumentSubmissionUpdateRequest request)
        {
            try
            {
                var claimId = Guid.Parse(documentSubmission.ClaimId);
                var claimUpdateRequest = new ClaimUpdateRequest { ValidUntil = DateTime.Parse(request.ValidUntil, new CultureInfo("en-GB")) };
                var claim = await _documentsApiGateway.UpdateClaim(claimId, claimUpdateRequest).ConfigureAwait(true);
                return claim;
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        private void NotifyResident(DocumentSubmission documentSubmission, DocumentSubmissionUpdateRequest request)
        {
            documentSubmission.RejectionReason = request.RejectionReason;

            EvidenceRequest evidenceRequest = new EvidenceRequest();
            if (documentSubmission.EvidenceRequestId != null)
            {
                evidenceRequest = _evidenceGateway.FindEvidenceRequest((Guid) documentSubmission.EvidenceRequestId);
            }

            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {documentSubmission.EvidenceRequestId}");
            }

            var resident = _residentsGateway.FindResident(evidenceRequest.ResidentId);

            if (resident == null)
            {
                throw new NotFoundException($"Cannot find resident with id: {evidenceRequest.ResidentId}");
            }

            try
            {
                documentSubmission.EvidenceRequest.DeliveryMethods.ForEach(dm =>
                    _notifyGateway.SendNotificationEvidenceRejected(dm, CommunicationReason.EvidenceRejected, documentSubmission, resident));
            }
            catch (NotifyClientException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new NotificationException(ex.Message);
            }
        }

        private DocumentType GetStaffSelectedDocumentType(DocumentSubmission documentSubmission, DocumentSubmissionUpdateRequest request)
        {
            DocumentType staffSelectedDocumentType = null;
            documentSubmission.StaffSelectedDocumentTypeId = request.StaffSelectedDocumentTypeId;

            EvidenceRequest evidenceRequest = new EvidenceRequest();
            if (documentSubmission.EvidenceRequestId != null)
            {
                evidenceRequest = _evidenceGateway.FindEvidenceRequest((Guid) documentSubmission.EvidenceRequestId);
            }

            staffSelectedDocumentType = _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(
                evidenceRequest.Team, request.StaffSelectedDocumentTypeId);
            return staffSelectedDocumentType;
        }
    }
}
