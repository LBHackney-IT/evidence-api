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
    public class UpdateDocumentSubmissionVisibilityUseCase : IUpdateDocumentSubmissionVisibiltyUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly ILogger<UpdateDocumentSubmissionVisibilityUseCase> _logger;

        public UpdateDocumentSubmissionVisibilityUseCase(
            IDocumentTypeGateway documentTypeGateway,
            IEvidenceGateway evidenceGateway,
            ILogger<UpdateDocumentSubmissionVisibilityUseCase> logger
        )
        {
            _documentTypeGateway = documentTypeGateway;
            _evidenceGateway = evidenceGateway;
            _logger = logger;
        }

        public void ExecuteAsync(Guid id, DocumentSubmissionVisibilityUpdateRequest request)
        {
            var documentSubmission = _evidenceGateway.FindAnyDocumentSubmission(id);

            if (documentSubmission == null)
            {
                throw new NotFoundException($"Cannot find document submission with id: {id}");
            }

            documentSubmission.IsHidden = request.DocumentHidden;
            _evidenceGateway.UpdateVisibilityDocumentSubmission(documentSubmission.Id, request.DocumentHidden);

        }
    }
}
