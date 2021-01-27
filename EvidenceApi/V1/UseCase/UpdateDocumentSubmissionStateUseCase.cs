using System;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCase : IUpdateDocumentSubmissionStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;

        public UpdateDocumentSubmissionStateUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
        }

        public DocumentSubmissionResponse Execute(Guid id, DocumentSubmissionRequest request)
        {
            var documentSubmission = _evidenceGateway.FindDocumentSubmission(id);

            if (documentSubmission == null)
            {
                throw new NotFoundException($"Cannot find document submission with id: {id}");
            }

            SubmissionState state;
            if (!Enum.TryParse(request.State, true, out state))
            {
                throw new BadRequestException("This state is invalid");
            }

            documentSubmission.State = state;
            _evidenceGateway.CreateDocumentSubmission(documentSubmission);

            var documentType = _documentTypeGateway.GetDocumentTypeById(documentSubmission.DocumentTypeId);
            return documentSubmission.ToResponse(documentType);
        }
    }
}
