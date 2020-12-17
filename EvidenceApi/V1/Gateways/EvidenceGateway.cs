using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.UseCase.Interfaces;

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
            var entity = request.ToEntity();
            _databaseContext.EvidenceRequests.Add(entity);
            _databaseContext.SaveChanges();

            request.Id = entity.Id;
            request.CreatedAt = entity.CreatedAt;
            return request;
        }

        public Communication CreateCommunication(Communication request)
        {
            var entity = request.ToEntity();
            _databaseContext.Communications.Add(entity);
            _databaseContext.SaveChanges();

            return entity.ToDomain();
        }

        public EvidenceRequest FindEvidenceRequest(Guid id)
        {
            var evidenceRequest = _databaseContext.EvidenceRequests.Find(id);

            if (evidenceRequest == null)
            {
                return null;
            }

            return evidenceRequest.ToDomain();
        }
    }
}
