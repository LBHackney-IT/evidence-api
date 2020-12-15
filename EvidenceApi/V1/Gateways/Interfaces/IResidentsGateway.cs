using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IResidentsGateway
    {
        Resident FindOrCreateResident(ResidentRequest request);
        Resident FindResident(Guid id);
    }
}
