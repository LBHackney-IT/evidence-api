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
                DocumentTypes = domain.DocumentTypeIds,
                ServiceRequestedBy = domain.ServiceRequestedBy,
                UserRequestedBy = domain.UserRequestedBy,
                ResidentId = domain.ResidentId
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

        public static CommunicationEntity ToEntity(this Communication domain)
        {
            return new CommunicationEntity()
            {
                CreatedAt = domain.CreatedAt,
                DeliveryMethod = domain.DeliveryMethod.ToString(),
                EvidenceRequestId = domain.EvidenceRequestId,
                Id = domain.Id,
                Reason = domain.Reason.ToString(),
                NotifyId = domain.NotifyId,
                TemplateId = domain.TemplateId
            };
        }

        public static DocumentSubmissionEntity ToEntity(this DocumentSubmission domain)
        {
            return new DocumentSubmissionEntity()
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                ClaimId = domain.ClaimId,
                RejectionReason = domain.RejectionReason,
                State = domain.State,
                EvidenceRequestId = domain.EvidenceRequest.Id,
                EvidenceRequest = domain.EvidenceRequest.ToEntity(),
                DocumentTypeId = domain.DocumentTypeId
            };
        }
    }
}
