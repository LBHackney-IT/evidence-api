using System;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCase : IUpdateDocumentSubmissionStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;
        private readonly IUpdateEvidenceRequestStateUseCase _updateEvidenceRequestStateUseCase;

        public UpdateDocumentSubmissionStateUseCase(
            IEvidenceGateway evidenceGateway,
            IDocumentTypeGateway documentTypeGateway,
            IStaffSelectedDocumentTypeGateway selectedDocumentTypeGateway,
            IUpdateEvidenceRequestStateUseCase updateEvidenceRequestStateUseCase
        )
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = selectedDocumentTypeGateway;
            _updateEvidenceRequestStateUseCase = updateEvidenceRequestStateUseCase;
        }

        public DocumentSubmissionResponse Execute(Guid id, DocumentSubmissionRequest request)
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

            DocumentType staffSelectedDocumentType = null;
            if (!String.IsNullOrEmpty(request.StaffSelectedDocumentTypeId))
            {
                documentSubmission.StaffSelectedDocumentTypeId = request.StaffSelectedDocumentTypeId;
                var evidenceRequest = _evidenceGateway.FindEvidenceRequest(documentSubmission.EvidenceRequestId);
                staffSelectedDocumentType = _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentId(
                    evidenceRequest.ServiceRequestedBy, request.StaffSelectedDocumentTypeId);
            }
            _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            _updateEvidenceRequestStateUseCase.Execute(documentSubmission.EvidenceRequestId);

            var documentType = _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentId(documentSubmission.EvidenceRequest.ServiceRequestedBy, documentSubmission.DocumentTypeId);
            return documentSubmission.ToResponse(documentType, staffSelectedDocumentType);
        }
    }
}
