using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IDocumentsApiGateway
    {
        Task<Claim> GetClaim(ClaimRequest request);
    }
}
