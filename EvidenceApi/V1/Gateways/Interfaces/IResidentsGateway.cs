using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IResidentsGateway
    {
        Resident FindOrCreateResident(Resident request);
        Resident FindResident(Guid id);
        Resident FindResident(Resident request);
        List<Resident> FindResidents(string searchQuery);
        Resident CreateResident(Resident request);
        void AddResidentGroupId(Resident request);
        List<GroupResidentIdClaimIdBackfillObject> GetAllResidentIdsAndGroupIdsByFirstCharacter(string groupIdFirstTwoCharacters);
    }
}
