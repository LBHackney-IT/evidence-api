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

        public UpdateDocumentSubmissionStateUseCase(IEvidenceGateway evidenceGateway)
        {
            _evidenceGateway = evidenceGateway;
        }

        public DocumentSubmissionResponse Execute(Guid id, DocumentSubmissionRequest request)
        {
            var documentSubmission = _evidenceGateway.FindDocumentSubmission(id);

            if (documentSubmission == null)
            {
                throw new NotFoundException($"Cannot find document submission with id: {id}");
            }

            // see if needed after adding e2e tests
            SubmissionState state;
            if (!Enum.TryParse<SubmissionState>(request.State, true, out state))
            {
                throw new BadRequestException("This state is invalid");
            }

            documentSubmission.State = state;

            _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            return documentSubmission.ToResponse();
        }
    }
}
