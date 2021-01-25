using System;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Infrastructure.Interfaces;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.V1.Domain
{
    [Table("document_submissions")]
    public class DocumentSubmission : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("claim_id")]
        public string ClaimId { get; set; }

        [Column("rejection_reason")]
        public string RejectionReason { get; set; }

        [Column("state")]
        public SubmissionState State { get; set; }

        [Column("evidence_request_id")]
        [ForeignKey("EvidenceRequest")]
        public Guid EvidenceRequestId { get; set; }

        [ForeignKey("EvidenceRequestId")]
        public virtual EvidenceRequest EvidenceRequest { get; set; }

        [Column("document_type_id")]
        public string DocumentTypeId { get; set; }

    }
}
