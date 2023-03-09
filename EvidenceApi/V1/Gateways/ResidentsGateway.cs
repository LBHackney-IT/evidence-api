using System;
using System.Collections.Generic;
using System.Linq;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.V1.Gateways
{
    // TODO: This gateway should be pointed at a Residents API when one exists
    public class ResidentsGateway : IResidentsGateway
    {
        private readonly EvidenceContext _databaseContext;

        public ResidentsGateway(EvidenceContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Resident FindOrCreateResident(Resident request)
        {
            var found = FindResident(request);
            if (found != null)
            {
                return found;
            }

            _databaseContext.Residents.Add(request);
            _databaseContext.SaveChanges();

            return request;
        }

        public Resident FindResident(Guid id)
        {
            return _databaseContext.Residents.Find(id);
        }

        public List<Resident> FindResidents(string searchQuery)
        {
            /*
             * I decided to use LINQ filtering over a DbSet because it seemed neat and meant we didn't need to worry about coding against SQL injection.
             * If we notice it is performing poorly then we could instead use raw SQL queries with named parameters.
             * See - https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
             */
            var searchQueryLowerCase = searchQuery?.ToLower();
            return _databaseContext.Residents
                .Where(r =>
                    r.Name.ToLower().StartsWith(searchQueryLowerCase) ||
                    r.Email.ToLower().StartsWith(searchQueryLowerCase) ||
                    r.PhoneNumber.ToLower().StartsWith(searchQueryLowerCase))
                .ToList();
        }

        public Resident FindResident(Resident request)
        {
            return _databaseContext.Residents
                .FirstOrDefault(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber &&
                    !r.IsHidden);
        }


        public Resident FindResidentByGroupId(ResidentSearchQuery request)
        {
            var residentTeamGroupIds = _databaseContext.ResidentsTeamGroupId
                .Where(r =>
                    r.GroupId == request.GroupId).Select(w => w.ResidentId).ToArray();

            if (residentTeamGroupIds.Length==0)
            {
                return null;
            }
            var resident = _databaseContext.Residents
                .FirstOrDefault(r => residentTeamGroupIds.Contains(r.Id) && !r.IsHidden);
            return resident;
        }

        public Resident CreateResident(Resident request)
        {
            _databaseContext.Residents.Add(request);
            _databaseContext.SaveChanges();
            return request;
        }

        public Guid AddResidentGroupId(Guid residentId, string team, Guid? groupId)
        {
            var newGroupId = groupId ?? Guid.NewGuid();
            var newEntry = new ResidentsTeamGroupId() { ResidentId = residentId, GroupId = newGroupId, Team = team };

            _databaseContext.ResidentsTeamGroupId.Add(newEntry);
            _databaseContext.SaveChanges();
            return newGroupId;
        }

        public Guid? FindGroupIdByResidentIdAndTeam(Guid residentId, string team)
        {
            var entity = _databaseContext.ResidentsTeamGroupId
                .FirstOrDefault(residentTeamGroupId =>
                    residentTeamGroupId.ResidentId == residentId &&
                    residentTeamGroupId.Team == team);
            if (entity == null)
            {
                return null;
            }
            return entity.GroupId;
        }

        public ResidentsTeamGroupId FindResidentTeamGroupIdByResidentIdAndTeam(Guid residentId, string team)
        {
            var entity = _databaseContext.ResidentsTeamGroupId
                .FirstOrDefault(r =>
                    r.ResidentId == residentId &&
                     r.Team == team);
            return entity;
        }

        public List<ResidentsTeamGroupId> FindResidentTeamGroupIdsByResidentId(Guid residentId)
        {
            var entities = _databaseContext.ResidentsTeamGroupId
                .Where(r =>
                    r.ResidentId == residentId).ToList();
            return entities;
        }

        public ResidentsTeamGroupId UpdateResidentGroupId(Guid residentId, string team, Guid groupId)
        {
            var entity = _databaseContext.ResidentsTeamGroupId.
                FirstOrDefault(r =>
                    r.ResidentId == residentId &&
                    r.Team == team);
            entity.GroupId = groupId;
            _databaseContext.SaveChanges();
            return entity;
        }

        public List<GroupResidentIdClaimIdBackfillObject> GetAllResidentIdsAndGroupIdsByFirstCharacter(string groupIdCharacters)

        {
            var result = new List<GroupResidentIdClaimIdBackfillObject>();

            var totalResidents = _databaseContext.ResidentsTeamGroupId.
                Where(x => x.GroupId.ToString().Substring(0, groupIdCharacters.Length) == groupIdCharacters).
                OrderByDescending(x => x.CreatedAt).ToList();

            foreach (var record in totalResidents)
            {
                GroupResidentIdClaimIdBackfillObject recordObject = new GroupResidentIdClaimIdBackfillObject()
                {
                    GroupId = record.GroupId,
                    ResidentId = record.ResidentId
                };
                result.Add(recordObject);
            }

            return result;
        }

        public Resident HideResident(Guid residentId)
        {
            var resident = FindResident(residentId);
            resident.IsHidden = true;
            _databaseContext.SaveChanges();
            return resident;
        }
    }
}
