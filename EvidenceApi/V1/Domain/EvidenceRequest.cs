using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Domain
{
    public class EvidenceRequest
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ResidentId { get; set; }
        public List<DeliveryMethod> DeliveryMethods { get; set; }
        public List<string> DocumentTypeIds { get; set; }
        public string ServiceRequestedBy { get; set; }
    }
}
