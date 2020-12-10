using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.V1.Factories
{
    public static class EntityFactory
    {
        public static EvidenceRequestEntity ToEntity(this EvidenceRequest domain)
        {
            return new EvidenceRequestEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                DeliveryMethods = domain.DeliveryMethods.ConvertAll(x => x.ToString()),
                DocumentTypes = domain.DocumentTypes.ConvertAll(x => x.Id),
                ServiceRequestedBy =  domain.ServiceRequestedBy,
                ResidentId = domain.Resident.Id
            };
        }

        public static ResidentEntity ToEntity(this Resident domain)
        {
            return new ResidentEntity()
            {
                Email = domain.Email,
                Name = domain.Name,
                PhoneNumber = domain.PhoneNumber,
                Id = domain.Id,
                CreatedAt = domain.CreatedAt
            };
        }
    }
}
