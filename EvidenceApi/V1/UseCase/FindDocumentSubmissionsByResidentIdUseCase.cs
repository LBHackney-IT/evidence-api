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

        public async Task<List<DocumentSubmissionResponse>> ExecuteAsync(DocumentSubmissionSearchQuery request)
        {
            ValidateRequest(request);

            EvidenceRequestsSearchQuery evidenceRequestSearchQuery = new EvidenceRequestsSearchQuery()
            {
                Team = request.Team,
                ResidentId = request.ResidentId
            };
            var evidenceRequests = _evidenceGateway.GetEvidenceRequests(evidenceRequestSearchQuery);

            var result = new List<DocumentSubmissionResponse>();

            foreach (var evidenceReq in evidenceRequests)
            {
                var documentSubmissions = _evidenceGateway.FindDocumentSubmissionsByEvidenceRequestId(evidenceReq.Id);
                foreach (var ds in documentSubmissions)
                {
                    var documentType = FindDocumentType(evidenceReq.Team, ds.DocumentTypeId);
                    var staffSelectedDocumentType = FindStaffSelectedDocumentType(evidenceReq.Team,
                        ds.StaffSelectedDocumentTypeId);
                    var claim = await _documentsApiGateway.GetClaimById(ds.ClaimId).ConfigureAwait(true);
                    {
                        result.Add(ds.ToResponse(documentType, staffSelectedDocumentType, claim));
                    }
                }
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
