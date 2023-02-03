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
        Guid AddResidentGroupId(Guid residentId, string team);
        Guid? FindGroupIdByResidentIdAndTeam(Guid residentId, string team);
        List<GroupResidentIdClaimIdBackfillObject> GetAllResidentIdsAndGroupIdsByFirstCharacter(char groupIdCharacter);
    }
}
