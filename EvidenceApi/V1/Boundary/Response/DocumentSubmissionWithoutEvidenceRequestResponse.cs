using System;
using EvidenceApi.V1.Domain;
#nullable enable annotations

namespace EvidenceApi.V1.Boundary.Response
{
    public class DocumentSubmissionWithoutEvidenceRequestResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimId { get; set; }
        public string Team { get; set; }
        public Guid ResidentId { get; set; }
        public DateTime ClaimValidUntil { get; set; }
        public DateTime RetentionExpiresAt { get; set; }
        public string State { get; set; }
        public DocumentType StaffSelectedDocumentType { get; set; }
        public S3UploadPolicy UploadPolicy { get; set; }
        public Document Document { get; set; }
    }
}
