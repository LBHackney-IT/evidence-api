using System;
using System.Collections.Generic;
using System.Linq;
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
            return _databaseContext.Residents
                .Where(r =>
                    r.Name.StartsWith(searchQuery) ||
                    r.Email.StartsWith(searchQuery) ||
                    r.PhoneNumber.StartsWith(searchQuery))
                .ToList();
        }

        private Resident FindResident(Resident request)
        {
            return _databaseContext.Residents
                .FirstOrDefault(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber);
        }
    }

}
