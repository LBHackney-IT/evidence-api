using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class ClaimRequest
    {
        public string ServiceAreaCreatedBy { get; set; }
        public string UserCreatedBy { get; set; }
        public string ApiCreatedBy { get; set; }
        public DateTime RetentionExpiresAt { get; set; }
    }
}
