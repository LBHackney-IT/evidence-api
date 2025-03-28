using System;
using EvidenceApi.V1.Domain;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IEvidenceGateway
    {
        AuditEvent CreateAuditEvent(AuditEvent request);
        EvidenceRequest CreateEvidenceRequest(EvidenceRequest request);
        EvidenceRequest FindEvidenceRequest(Guid id);
        EvidenceRequest FindEvidenceRequestWithDocumentSubmissions(Guid id);
        Communication CreateCommunication(Communication request);
        DocumentSubmission CreateDocumentSubmission(DocumentSubmission request);
        DocumentSubmission FindDocumentSubmission(Guid id);
        DocumentSubmission FindAnyDocumentSubmission(Guid id);
        List<EvidenceRequest> GetEvidenceRequests(EvidenceRequestsSearchQuery request);
        List<EvidenceRequest> GetEvidenceRequestsWithDocumentSubmissions(EvidenceRequestsSearchQuery request);
        List<EvidenceRequest> FindEvidenceRequestsByResidentId(Guid id);
        List<EvidenceRequest> GetAll();
        List<EvidenceRequest> GetEvidenceRequests(ResidentSearchQuery request);
        DocumentSubmissionQueryResponse GetPaginatedDocumentSubmissionsByResidentId(Guid id, string team, SubmissionState? state,
            int? pageSize, int? page);
        List<GroupResidentIdClaimIdBackfillObject> GetClaimIdsForResidentsWithGroupIds(
            List<GroupResidentIdClaimIdBackfillObject> groupResidentIdClaimIdBackfillObject);

        void UpdateResidentIdForDocumentSubmission(Guid newResidentId, Guid[] residentIds);
        void UpdateVisibilityDocumentSubmission(Guid documentSubmissionId, bool visibility);

    }
}
