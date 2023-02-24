using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class ResidentGroupIdRequest
    {
        public Guid ResidentId { get; set; }
        public string Team { get; set; }
        public Guid GroupId { get; set; }
    }
}
