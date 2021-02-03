using System;
using EvidenceApi.V1.Domain;
using System.Collections.Generic;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IEvidenceGateway
    {
        EvidenceRequest CreateEvidenceRequest(EvidenceRequest request);
        EvidenceRequest FindEvidenceRequest(Guid id);
        Communication CreateCommunication(Communication request);
        DocumentSubmission CreateDocumentSubmission(DocumentSubmission request);
        DocumentSubmission FindDocumentSubmission(Guid id);
        List<EvidenceRequest> GetEvidenceRequests(string serviceRequestedBy, Guid? residentId = null);
    }
}
