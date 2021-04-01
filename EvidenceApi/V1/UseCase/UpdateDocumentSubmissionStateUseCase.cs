using System;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Domain.Enums;
using Notify.Exceptions;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCase : IUpdateDocumentSubmissionStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;
        private readonly IUpdateEvidenceRequestStateUseCase _updateEvidenceRequestStateUseCase;
        private readonly INotifyGateway _notifyGateway;
        private readonly IResidentsGateway _residentsGateway;

        public UpdateDocumentSubmissionStateUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentTypeGateway documentTypeGateway,
            IStaffSelectedDocumentTypeGateway selectedDocumentTypeGateway,
            INotifyGateway notifyGateway,
            IResidentsGateway residentsGateway,
            IUpdateEvidenceRequestStateUseCase updateEvidenceRequestStateUseCase
        )
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = selectedDocumentTypeGateway;
            _updateEvidenceRequestStateUseCase = updateEvidenceRequestStateUseCase;
            _notifyGateway = notifyGateway;
            _residentsGateway = residentsGateway;
        }

        public DocumentSubmissionResponse Execute(Guid id, DocumentSubmissionUpdateRequest request)
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
            _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            _updateEvidenceRequestStateUseCase.Execute(documentSubmission.EvidenceRequestId);

            var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(documentSubmission.EvidenceRequest.ServiceRequestedBy, documentSubmission.DocumentTypeId);
            return documentSubmission.ToResponse(documentType, staffSelectedDocumentType);
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
                Console.Error.WriteLine(ex);
                throw new NotificationException(documentSubmission.EvidenceRequest);
            }
        }

        private DocumentType GetStaffSelectedDocumentType(DocumentSubmission documentSubmission, DocumentSubmissionUpdateRequest request)
        {
            DocumentType staffSelectedDocumentType = null;
            documentSubmission.StaffSelectedDocumentTypeId = request.StaffSelectedDocumentTypeId;
            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(documentSubmission.EvidenceRequestId);
            staffSelectedDocumentType = _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(
                evidenceRequest.ServiceRequestedBy, request.StaffSelectedDocumentTypeId);
            return staffSelectedDocumentType;
        }
    }
}
