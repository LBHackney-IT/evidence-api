using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionSearchQuery
    {
        public string ServiceRequestedBy { get; set; }
        public Guid ResidentId { get; set; }
    }
}
