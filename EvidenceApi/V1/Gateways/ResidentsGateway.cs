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

        public Resident FindOrCreateResident(ResidentRequest request)
        {
            var found = FindResident(request);
            if (found != null)
            {
                return EntityToDomain(found);
            }

            var entity = new ResidentEntity()
            {
                Email = request.Email, Name = request.Name, PhoneNumber = request.PhoneNumber
            };
            _databaseContext.Residents.Add(entity);
            _databaseContext.SaveChanges();

            return EntityToDomain(FindResident(request));
        }

        private ResidentEntity FindResident(ResidentRequest request)
        {
            return _databaseContext.Residents
                .FirstOrDefault(r =>
                    r.Name == request.Name &&
                    r.Email == request.Email &&
                    r.PhoneNumber == request.PhoneNumber);
        }

        private static Resident EntityToDomain(ResidentEntity entity)
        {
            return new Resident() {
                Email = entity.Email, Id = entity.Id, Name = entity.Name, PhoneNumber = entity.PhoneNumber
            };
        }
    }

}
