using System.Collections.Generic;

namespace EvidenceApi.V1.Boundary.Request
{
    public class EvidenceRequestRequest
    {
        public ResidentRequest Resident { get; set; }
        public List<string> DeliveryMethods { get; set; }
        public List<string> DocumentTypes { get; set; }
        public string ServiceRequestedBy { get; set; }
        public string Reason { get; set; }
        public string UserRequestedBy { get; set; }
    }
}
