using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.V1.Factories
{
    public static class DomainFactory
    {
        public static Resident ToDomain(this ResidentEntity entity)
        {
            return new Resident()
            {
                Email = entity.Email,
                Id = entity.Id,
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                CreatedAt = entity.CreatedAt
            };
        }

        public static Communication ToDomain(this CommunicationEntity entity)
        {
            return new Communication()
            {
                CreatedAt = entity.CreatedAt,
                Id = entity.Id,
                DeliveryMethod = ParseDeliveryMethod(entity.DeliveryMethod),
                NotifyId = entity.NotifyId,
                TemplateId = entity.TemplateId,
                EvidenceRequestId = entity.EvidenceRequest.Id
            };
        }

        public static EvidenceRequest ToDomain(this EvidenceRequestEntity entity)
        {
            return new EvidenceRequest
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                DocumentTypeIds = entity.DocumentTypes,
                DeliveryMethods = entity.DeliveryMethods.ConvertAll(ParseDeliveryMethod),
                ServiceRequestedBy = entity.ServiceRequestedBy,
                ResidentId = entity.ResidentId
            };
        }
        public static DocumentSubmission ToDomain(this DocumentSubmissionEntity entity)
        {
            return new DocumentSubmission
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                ClaimId = entity.ClaimId,
                RejectionReason = entity.RejectionReason,
                State = entity.State,
                EvidenceRequest = entity.EvidenceRequest?.ToDomain(),
                DocumentTypeId = entity.DocumentTypeId
            };
        }

        private static DeliveryMethod ParseDeliveryMethod(string deliveryMethod)
        {
            return Enum.Parse<DeliveryMethod>(deliveryMethod, true);
        }
    }
}
