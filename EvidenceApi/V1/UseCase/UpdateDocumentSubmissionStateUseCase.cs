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
                throw new BadRequestException("Document has already been approved/rejected");
            }

            documentSubmission.State = state;

            if (!String.IsNullOrEmpty(request.RejectionReason) && documentSubmission.State == SubmissionState.Rejected)
            {
                NotifyResident(documentSubmission, request);
            }

            DocumentType staffSelectedDocumentType = null;
            if (!String.IsNullOrEmpty(request.StaffSelectedDocumentTypeId))
            {
                staffSelectedDocumentType = GetStaffSelectedDocumentType(documentSubmission, request);
            }

            if (!String.IsNullOrEmpty(request.ValidUntil))
            {
                await UpdateClaim(documentSubmission, request).ConfigureAwait(true);
            }

            _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            _updateEvidenceRequestStateUseCase.Execute(documentSubmission.EvidenceRequestId);

            var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(documentSubmission.EvidenceRequest.Team, documentSubmission.DocumentTypeId);
            return documentSubmission.ToResponse(documentType, staffSelectedDocumentType);
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
                throw new BadRequestException($"Issue with DocumentsApi so cannot update claim: {ex.Message}");
            }
        }

        private void NotifyResident(DocumentSubmission documentSubmission, DocumentSubmissionUpdateRequest request)
        {
            documentSubmission.RejectionReason = request.RejectionReason;
            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(documentSubmission.EvidenceRequestId);

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
                    _notifyGateway.SendNotification(dm, CommunicationReason.EvidenceRejected, documentSubmission, resident));
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
            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(documentSubmission.EvidenceRequestId);
            staffSelectedDocumentType = _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(
                evidenceRequest.Team, request.StaffSelectedDocumentTypeId);
            return staffSelectedDocumentType;
        }
    }
}
