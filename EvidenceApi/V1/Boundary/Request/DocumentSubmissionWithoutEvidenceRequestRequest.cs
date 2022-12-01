using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionWithoutEvidenceRequestRequest
    {
        public Guid ResidentId { get; set; }
        public string Team { get; set; }
        public string UserCreatedBy { get; set; }
        public string StaffSelectedDocumentTypeId { get; set; }
        public string DocumentDescription { get; set; }
    }
}
