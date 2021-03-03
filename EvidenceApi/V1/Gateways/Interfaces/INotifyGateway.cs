using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface INotifyGateway
    {
        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, EvidenceRequest request,
            Resident resident);

    }
}
