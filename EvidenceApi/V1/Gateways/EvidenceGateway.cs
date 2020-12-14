using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;

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
    }
}
