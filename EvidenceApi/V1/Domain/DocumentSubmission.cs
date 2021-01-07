using System;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Domain
{
    public class DocumentSubmission
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimId { get; set; }
        public string RejectionReason { get; set; }
        public SubmissionState State { get; set; }
        public EvidenceRequest EvidenceRequest { get; set; }
        public string DocumentTypeId { get; set; }
    }
}
