using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static EvidenceRequestResponse ToResponse(this EvidenceRequest domain, Resident resident, List<DocumentType> documentTypes)
        {
            return new EvidenceRequestResponse()
            {
                Resident = resident.ToResponse(),
                DeliveryMethods = domain.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()),
                DocumentTypes = documentTypes,
                ServiceRequestedBy = domain.ServiceRequestedBy,
                Id = domain.Id,
                CreatedAt = domain.CreatedAt
            };
        }

        public static ResidentResponse ToResponse(this Resident domain)
        {
            return new ResidentResponse()
            {
                Id = domain.Id,
                Name = domain.Name,
                Email = domain.Email,
                PhoneNumber = domain.PhoneNumber
            };
        }
    }
}
