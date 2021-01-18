using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;

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

        public DocumentSubmissionResponse Execute(Guid evidenceRequestId, DocumentSubmissionRequest request)
        {
            if (String.IsNullOrEmpty(request.DocumentType))
            {
                throw new BadRequestException("Document type is null or empty");
            }

            if (String.IsNullOrEmpty(request.ServiceName))
            {
                throw new BadRequestException("Service name is null or empty");
            }

            if (String.IsNullOrEmpty(request.RequesterEmail))
            {
                throw new BadRequestException("Requester email is null or empty");
            }

            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            var claimRequest = new ClaimRequest()
            {
                ServiceAreaCreatedBy = request.ServiceName,
                UserCreatedBy = request.RequesterEmail,
                ApiCreatedBy = "evidence_api",
                RetentionExpiresAt = DateTime.Now.AddMonths(3)
            };

            var claim = _documentsApiGateway.GetClaim(claimRequest);

            var documentSubmission = new DocumentSubmission()
            {
                EvidenceRequest = evidenceRequest,
                DocumentTypeId = request.DocumentType,
                ClaimId = claim.Id.ToString()
            };

            var createdDocumentSubmission = _evidenceGateway.CreateDocumentSubmission(documentSubmission);
            return createdDocumentSubmission.ToResponse(request.DocumentType);
        }
    }
}
