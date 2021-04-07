using System;
using EvidenceApi.V1.Domain;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IEvidenceGateway
    {
        EvidenceRequest CreateEvidenceRequest(EvidenceRequest request);
        EvidenceRequest FindEvidenceRequest(Guid id);
        Communication CreateCommunication(Communication request);
        DocumentSubmission CreateDocumentSubmission(DocumentSubmission request);
        DocumentSubmission FindDocumentSubmission(Guid id);
        List<EvidenceRequest> GetEvidenceRequests(EvidenceRequestsSearchQuery request);
        List<DocumentSubmission> FindDocumentSubmissionsByEvidenceRequestId(Guid id);
        List<EvidenceRequest> FindEvidenceRequestsByResidentId(Guid id);
        List<EvidenceRequest> GetAll();
        List<EvidenceRequest> GetEvidenceRequests(ResidentSearchQuery request);
    }
}
