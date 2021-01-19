using System;
using EvidenceApi.V1.Domain.Enums;
#nullable enable annotations
namespace EvidenceApi.V1.Domain
{
    public class DocumentSubmission
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimId { get; set; }
        public string RejectionReason { get; set; }
        public SubmissionState State { get; set; } = SubmissionState.pending;
        public EvidenceRequest EvidenceRequest { get; set; }
        public string DocumentTypeId { get; set; }
        public S3UploadPolicy? UploadPolicy { get; set; }
    }
}
