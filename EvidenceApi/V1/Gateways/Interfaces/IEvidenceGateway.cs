using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IEvidenceGateway
    {
        EvidenceRequest CreateEvidenceRequest(EvidenceRequest request);
        Communication CreateCommunication(Communication request);
    }
}
