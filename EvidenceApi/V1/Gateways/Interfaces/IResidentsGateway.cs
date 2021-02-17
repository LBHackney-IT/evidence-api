using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IResidentsGateway
    {
        Resident FindOrCreateResident(Resident request);
        Resident FindResident(Guid id);
        List<Resident> FindResidents(string searchQuery);
    }
}
