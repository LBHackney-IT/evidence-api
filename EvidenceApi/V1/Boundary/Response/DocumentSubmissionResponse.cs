using System;
using EvidenceApi.V1.Domain;
#nullable enable annotations

namespace EvidenceApi.V1.Boundary.Response
{
    public class DocumentSubmissionResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimId { get; set; }
        public string RejectionReason { get; set; }
        public string State { get; set; }
        public DocumentType DocumentType { get; set; }
        public DocumentType? StaffSelectedDocumentType { get; set; }
        public S3UploadPolicy? UploadPolicy { get; set; }
        public Document? Document { get; set; }
    }
}
