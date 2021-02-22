using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using System.Collections.Generic;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using System.Linq;
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
            EvidenceRequestsSearchQuery evidenceRequestSearchQuery = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = request.serviceRequestedBy,
                ResidentId = request.ResidentId
            };
            var found = _evidenceGateway.GetEvidenceRequests(evidenceRequestSearchQuery);

            if (found == null)
            {
                throw new NotFoundException("Cannot find evidence requests with provided parameters");
            }

            var documentSubmissions = new List<DocumentSubmission>();

            foreach (var er in found)
            {
                documentSubmissions = documentSubmissions.Concat(_evidenceGateway.FindDocumentSubmissionByEvidenceRequestId(er.Id)).ToList();
            }

            var result = new List<DocumentSubmissionResponse>();

            foreach (var ds in documentSubmissions)
            {
                var documentType = FindDocumentType(ds.DocumentTypeId);
                var claim = await _documentsApiGateway.GetClaimById(ds.ClaimId).ConfigureAwait(true);
                if (claim.Document == null)
                {
                    result.Add(ds.ToResponse(documentType, null, null));
                    return result;
                }
                result.Add(ds.ToResponse(documentType, null, claim.Document));
            }
            return result;
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }
    }
}
