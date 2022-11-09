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
        private readonly IStaffSelectedDocumentTypeGateway _staffSelectedDocumentTypeGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;

        public FindDocumentSubmissionsByResidentIdUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway, IDocumentsApiGateway documentsApiGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
            _documentsApiGateway = documentsApiGateway;
        }

        public async Task<DocumentSubmissionResponseObject> ExecuteAsync(DocumentSubmissionSearchQuery request)
        {
            ValidateRequest(request);

            var query =
                _evidenceGateway.GetPaginatedDocumentSubmissionsByResidentId(request.ResidentId, request?.PageSize, request?.Page);

            var result = new DocumentSubmissionResponseObject { Total = query.Total, DocumentSubmissions = new List<DocumentSubmissionResponse>() };

            var claimsIds = new List<string>();
            foreach (var ds in query.DocumentSubmissions)
            {
                claimsIds.Add(ds.ClaimId);
            }
            var claims = await _documentsApiGateway.GetClaimsByIdsThrottled(claimsIds);

            var claimIndex = 0;
            foreach (var ds in query.DocumentSubmissions)
            {
                var documentType = FindDocumentType(ds.Team, ds.DocumentTypeId);
                var staffSelectedDocumentType = FindStaffSelectedDocumentType(ds.Team,
                    ds.StaffSelectedDocumentTypeId);
                result.DocumentSubmissions.Add(ds.ToResponse(documentType, (Guid) ds.EvidenceRequestId, staffSelectedDocumentType, null, claims[claimIndex]));
                claimIndex++;
            }

            return result;
        }

        private DocumentType FindDocumentType(string teamName, string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, documentTypeId);
        }

        private DocumentType FindStaffSelectedDocumentType(string teamName, string staffSelectedDocumentTypeId)
        {
            return _staffSelectedDocumentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, staffSelectedDocumentTypeId);
        }

        private static void ValidateRequest(DocumentSubmissionSearchQuery request)
        {
            if (String.IsNullOrEmpty(request.Team))
            {
                throw new BadRequestException("Team is null or empty");
            }

            if (request.ResidentId == Guid.Empty)
            {
                throw new BadRequestException("Resident ID is invalid");
            }
        }
    }
}
