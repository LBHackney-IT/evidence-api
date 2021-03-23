using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using System.Collections.Generic;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using System.Threading.Tasks;
using System;

namespace EvidenceApi.V1.UseCase
{
    public class FindDocumentSubmissionsByResidentIdUseCase : IFindDocumentSubmissionsByResidentIdUseCase
    {
        private IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;

        public FindDocumentSubmissionsByResidentIdUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IDocumentsApiGateway documentsApiGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _documentsApiGateway = documentsApiGateway;
        }

        public async Task<List<DocumentSubmissionResponse>> ExecuteAsync(DocumentSubmissionSearchQuery request)
        {
            ValidateRequest(request);

            EvidenceRequestsSearchQuery evidenceRequestSearchQuery = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = request.ServiceRequestedBy,
                ResidentId = request.ResidentId
            };
            var evidenceRequests = _evidenceGateway.GetEvidenceRequests(evidenceRequestSearchQuery);

            var result = new List<DocumentSubmissionResponse>();

            foreach (var evidenceReq in evidenceRequests)
            {
                var documentSubmissions = _evidenceGateway.FindDocumentSubmissionByEvidenceRequestId(evidenceReq.Id);
                foreach (var ds in documentSubmissions)
                {
                    var documentType = FindDocumentType(evidenceReq.ServiceRequestedBy, ds.DocumentTypeId);
                    var claim = await _documentsApiGateway.GetClaimById(ds.ClaimId).ConfigureAwait(true);
                    if (claim.Document == null)
                    {
                        result.Add(ds.ToResponse(documentType, null, null));
                    }
                    else
                    {
                        result.Add(ds.ToResponse(documentType, null, claim.Document));
                    }
                }
            }
            return result;
        }

        private DocumentType FindDocumentType(string teamName, string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentId(teamName, documentTypeId);
        }

        private static void ValidateRequest(DocumentSubmissionSearchQuery request)
        {
            if (String.IsNullOrEmpty(request.ServiceRequestedBy))
            {
                throw new BadRequestException("Service requested by is null or empty");
            }

            if (request.ResidentId == Guid.Empty)
            {
                throw new BadRequestException("Resident ID is invalid");
            }
        }
    }
}
