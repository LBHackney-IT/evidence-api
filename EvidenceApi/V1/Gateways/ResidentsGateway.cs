using System;
using System.Linq;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.UseCase.Interfaces;

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

        public Resident FindOrCreateResident(ResidentRequest request)
        {
            var found = FindResident(request);
            if (found != null)
            {
                return found.ToDomain();
            }

            var entity = new ResidentEntity()
            {
                Email = request.Email,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber
            };
            _databaseContext.Residents.Add(entity);
            _databaseContext.SaveChanges();

            return entity.ToDomain();
        }

        public Resident FindResident(Guid id)
        {
            ResidentEntity found = _databaseContext.Residents.Find(id);
            return found.ToDomain();
        }

        private ResidentEntity FindResident(ResidentRequest request)
        {
            return _databaseContext.Residents
                .FirstOrDefault(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber);
        }
    }

}
