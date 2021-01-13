using System;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Boundary.Response
{
    public class DocumentSubmissionResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimId { get; set; }
        public string RejectionReason { get; set; }
        public string State { get; set; }
        public string DocumentType { get; set; }
    }
}
