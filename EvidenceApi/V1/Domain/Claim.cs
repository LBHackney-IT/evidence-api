using System;

namespace EvidenceApi.V1.Domain
{
    public class Claim
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Document Document { get; set; }
        public string ServiceAreaCreatedBy { get; set; }
        public string UserCreatedBy { get; set; }
        public string ApiCreatedBy { get; set; }
        public DateTime RetentionExpiresAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }

    public class Document
    {
        public Guid Id { get; set; }
        public int FileSize { get; set; }
        public string FileType { get; set; }
    }
}
