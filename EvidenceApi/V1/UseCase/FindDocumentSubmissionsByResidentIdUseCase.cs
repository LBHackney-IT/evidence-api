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
        private readonly IResidentsGateway _residentsGateway;

        public FindDocumentSubmissionsByResidentIdUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, IStaffSelectedDocumentTypeGateway staffSelectedDocumentTypeGateway, IDocumentsApiGateway documentsApiGateway, IResidentsGateway residentsGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _staffSelectedDocumentTypeGateway = staffSelectedDocumentTypeGateway;
            _documentsApiGateway = documentsApiGateway;
            _residentsGateway = residentsGateway;
        }

        public async Task<DocumentSubmissionResponseObject> ExecuteAsync(DocumentSubmissionSearchQuery request)
        {
            ValidateRequest(request);

            //get group Id

                //var groupId = _residentsGateway.GetGroupIdByResidentIdAndTeam(request);

          var query = _evidenceGateway.GetPaginatedDocumentSubmissionsByResidentId(request.ResidentId, request?.State, request?.PageSize, request?.Page);

            //create result
            var result = new DocumentSubmissionResponseObject { Total = query.Total, DocumentSubmissions = new List<DocumentSubmissionResponse>() };

            //we can use the groupId to do a query on the table to get all the claims
            var claimsIds = new List<string>();
            foreach (var ds in query.DocumentSubmissions)
            {
                claimsIds.Add(ds.ClaimId);
            }
            var claims = await _documentsApiGateway.GetClaimsByIdsThrottled(claimsIds);

            //build the object as before?
            var claimIndex = 0;
            foreach (var ds in query.DocumentSubmissions)
            {
                var documentType = FindDocumentType(ds.Team, ds.DocumentTypeId);
                var staffSelectedDocumentType = FindStaffSelectedDocumentType(ds.Team,
                    ds.StaffSelectedDocumentTypeId);
                result.DocumentSubmissions.Add(ds.ToResponse(documentType, ds.EvidenceRequestId, staffSelectedDocumentType, null, claims[claimIndex]));
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
