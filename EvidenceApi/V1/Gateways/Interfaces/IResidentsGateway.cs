using System;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;


namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IResidentsGateway
    {
        Resident FindOrCreateResident(Resident request);
        Resident FindResident(Guid id);
        Resident FindResident(Resident request);
        Resident FindResidentByGroupId(ResidentSearchQuery request);
        List<Resident> FindResidents(string searchQuery);
        Resident CreateResident(Resident request);
        List<GroupResidentIdClaimIdBackfillObject> GetAllResidentIdsAndGroupIdsByFirstCharacter(string groupIdFirstTwoCharacters);
        Guid AddResidentGroupId(Guid residentId, string team, Guid? groupId);
        Guid? FindGroupIdByResidentIdAndTeam(Guid residentId, string team);
        ResidentsTeamGroupId FindResidentTeamGroupIdByResidentIdAndTeam(Guid residentId, string team);
        ResidentsTeamGroupId UpdateResidentGroupId(Guid residentId, string team, Guid groupId);

    }
}
