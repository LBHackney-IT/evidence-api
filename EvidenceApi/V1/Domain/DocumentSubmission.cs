using System;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Infrastructure.Interfaces;
using EvidenceApi.V1.Domain.Enums;

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

        [Column("accepted_at")]
        public DateTime? AcceptedAt { get; set; }

        [Column("rejection_reason")]
        public string RejectionReason { get; set; }

        [Column("rejected_at")]
        public DateTime? RejectedAt { get; set; }

        [Column("user_updated_by")]
        public string UserUpdatedBy { get; set; }

        [Column("state")]
        public SubmissionState State { get; set; }

        [Column("evidence_request_id")]
        [ForeignKey("EvidenceRequest")]
        public Guid? EvidenceRequestId { get; set; } = null;

        [ForeignKey("EvidenceRequestId")]
        public virtual EvidenceRequest EvidenceRequest { get; set; }

        [Column("document_type_id")]
        public string DocumentTypeId { get; set; }

        [Column("staff_selected_document_type_id")]
        public string StaffSelectedDocumentTypeId { get; set; }

        [Column("resident_id")]
        [ForeignKey("Resident")]
        public Guid ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public virtual Resident Resident { get; set; }
        [Column("team")]
        public string Team { get; set; }
        [Column("is_hidden")]
        public bool isHidden { get; set; } = false;
    }
}
