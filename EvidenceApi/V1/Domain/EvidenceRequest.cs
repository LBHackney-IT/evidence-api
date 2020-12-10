using System;
using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class EvidenceRequest
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Resident Resident { get; set; }
        public List<DeliveryMethod> DeliveryMethods { get; set; }
        public List<DocumentType> DocumentTypes { get; set; }
        public string ServiceRequestedBy { get; set; }

        public enum DeliveryMethod
        {
            Sms,
            Email
        }
    }
}
