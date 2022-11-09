using System;
using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using System.Collections.Generic;
using Amazon.Runtime.Internal;
using EvidenceApi.V1.Boundary.Request;
using Microsoft.EntityFrameworkCore;

namespace EvidenceApi.V1.Gateways
{
    public class EvidenceGateway : IEvidenceGateway
    {
        private readonly EvidenceContext _databaseContext;

        public EvidenceGateway(EvidenceContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public AuditEvent CreateAuditEvent(AuditEvent request)
        {
            if (request.Id == default) _databaseContext.AuditEvents.Add(request);
            _databaseContext.SaveChanges();

            return request;
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

        public EvidenceRequest FindEvidenceRequestWithDocumentSubmissions(Guid id)
        {
            return _databaseContext.EvidenceRequests.Where(er => er.Id == id).Include(er => er.DocumentSubmissions).FirstOrDefault();
        }

        public List<EvidenceRequest> GetEvidenceRequests(EvidenceRequestsSearchQuery request)
        {
            return _databaseContext.EvidenceRequests
                .Where(x =>
                    x.Team.Equals(request.Team) &&
                    (request.ResidentId == null || x.ResidentId.Equals(request.ResidentId)) &&
                    (request.State == null || x.State.Equals(request.State))
                ).ToList();
        }

        public List<EvidenceRequest> GetEvidenceRequestsWithDocumentSubmissions(EvidenceRequestsSearchQuery request)
        {
            var orderdEvidenceRequestsAndOrderedDocSubmissions = _databaseContext.EvidenceRequests
                .Where(x =>
                    x.Team.Equals(request.Team) &&
                    (request.ResidentId == null || x.ResidentId.Equals(request.ResidentId)) &&
                    (request.State == null || x.State.Equals(request.State))
                ).Include(er => er.DocumentSubmissions)
                .OrderByDescending(er => er.CreatedAt).ToList();
            orderdEvidenceRequestsAndOrderedDocSubmissions.ForEach(er => er.DocumentSubmissions = er.DocumentSubmissions.OrderByDescending(ds => ds.CreatedAt).ToList());
            return orderdEvidenceRequestsAndOrderedDocSubmissions.ToList();
        }

        public DocumentSubmission FindDocumentSubmission(Guid id)
        {
            return _databaseContext.DocumentSubmissions.Find(id);
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
                    x.Team.Equals(request.Team) &&
                    x.ResidentReferenceId.Equals(request.SearchQuery)
                )
                .ToList();
        }

        public DocumentSubmissionQueryResponse GetPaginatedDocumentSubmissionsByResidentId(Guid id, int? limit = 10,
            int? page = 1)
        {
            var offset = (limit * page) - limit;

            var total = _databaseContext.DocumentSubmissions
                .Count(x => x.ResidentId.Equals(id));

            var documents = _databaseContext.DocumentSubmissions
                .Where(x => x.ResidentId.Equals(id))
                .Skip(offset ?? 0)
                .Take(limit ?? 10)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return new DocumentSubmissionQueryResponse() { DocumentSubmissions = documents, Total = total };

        }
    }
}
