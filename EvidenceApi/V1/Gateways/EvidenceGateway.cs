using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
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

        public EvidenceRequest CreateEvidenceRequest(EvidenceRequest request)
        {
            _databaseContext.EvidenceRequests.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public DocumentSubmission CreateDocumentSubmission(DocumentSubmission request)
        {
            _databaseContext.DocumentSubmissions.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public Communication CreateCommunication(Communication request)
        {
            _databaseContext.Communications.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public EvidenceRequest FindEvidenceRequest(Guid id)
        {
            return _databaseContext.EvidenceRequests.Find(id);
        }

        public DocumentSubmission FindDocumentSubmission(Guid id)
        {
            var documentSubmission = _databaseContext.DocumentSubmissions.Find(id);

            if (documentSubmission == null)
            {
                return null;
            }

            var domain = documentSubmission.ToDomain();
            _databaseContext.Entry(documentSubmission).State = EntityState.Detached;
            return domain;
        }
    }
}
