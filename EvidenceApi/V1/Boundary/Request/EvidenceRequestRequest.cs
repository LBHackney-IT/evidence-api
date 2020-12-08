using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Request
{
    public class EvidenceRequestRequest
    {
        public ResidentRequest Resident { get; set; }
        public List<string> DeliveryMethods { get; set; }
        public string DocumentType { get; set; }
        public string ServiceRequestedBy { get; set; }
    }
}
