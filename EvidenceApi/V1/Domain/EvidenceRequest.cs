using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class EvidenceRequest
    {
        public string Id { get; set; }
        public Resident Resident { get; set; }
        public List<DeliveryMethod> DeliveryMethods { get; set; }
        public string DocumentType { get; set; }
        public string ServiceRequestedBy { get; set; }

        public enum DeliveryMethod
        {
            Sms,
            Email
        }
    }
}
