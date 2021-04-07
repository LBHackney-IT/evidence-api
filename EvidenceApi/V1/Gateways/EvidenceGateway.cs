using System;
using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.Gateways
{
    public class EvidenceGateway : IEvidenceGateway
    {
        private readonly EvidenceContext _databaseContext;

        public EvidenceGateway(EvidenceContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public EvidenceRequest CreateEvidenceRequest(EvidenceRequest request)
        {
            if (request.Id == default) _databaseContext.EvidenceRequests.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public DocumentSubmission CreateDocumentSubmission(DocumentSubmission request)
        {
            if (request.Id == default) _databaseContext.DocumentSubmissions.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public Communication CreateCommunication(Communication request)
        {
            if (request.Id == default) _databaseContext.Communications.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public EvidenceRequest FindEvidenceRequest(Guid id)
        {
            return _databaseContext.EvidenceRequests.Find(id);
        }

        public List<EvidenceRequest> GetEvidenceRequests(EvidenceRequestsSearchQuery request)
        {
            return _databaseContext.EvidenceRequests
                .Where(x =>
                    x.ServiceRequestedBy.Equals(request.ServiceRequestedBy) &&
                    (request.ResidentId == null || x.ResidentId.Equals(request.ResidentId)) &&
                    (request.State == null || x.State.Equals(request.State))
                ).ToList();
        }

        public DocumentSubmission FindDocumentSubmission(Guid id)
        {
            return _databaseContext.DocumentSubmissions.Find(id);
        }

        public List<DocumentSubmission> FindDocumentSubmissionsByEvidenceRequestId(Guid id)
        {
            return _databaseContext.DocumentSubmissions.Where(x => x.EvidenceRequestId == id).ToList();
        }

        public List<EvidenceRequest> FindEvidenceRequestsByResidentId(Guid id)
        {
            return _databaseContext.EvidenceRequests.Where(x => x.ResidentId.Equals(id)).ToList();
        }

        public List<EvidenceRequest> GetAll()
        {
            return _databaseContext.EvidenceRequests.ToList();
        }

        public List<EvidenceRequest> GetEvidenceRequests(ResidentSearchQuery request)
        {
            return _databaseContext.EvidenceRequests
                .Where(x =>
                    x.ServiceRequestedBy.Equals(request.ServiceRequestedBy) &&
                    x.ResidentReferenceId.Equals(request.SearchQuery)
                ).ToList();
        }
    }
}
