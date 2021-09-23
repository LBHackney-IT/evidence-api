using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Domain
{
    [Table("evidence_requests")]
    public class EvidenceRequest : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("resident_id")]
        public Guid ResidentId { get; set; }

        [Column("resident_reference_id")]
        public string ResidentReferenceId { get; set; }

        [Column("delivery_methods")]
        public List<string> RawDeliveryMethods { get; set; }

        [Column("document_types")]
        public List<string> DocumentTypes { get; set; }

        [Column("team")]
        public string Team { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("user_requested_by")]
        public string UserRequestedBy { get; set; }

        [Column("state")]
        public EvidenceRequestState State { get; set; }

        [Column("notification_email")]
        public string NotificationEmail { get; set; }

        public virtual ICollection<Communication> Communications { get; set; }
        public virtual ICollection<DocumentSubmission> DocumentSubmissions { get; set; }

        [NotMapped]
        public virtual List<DeliveryMethod> DeliveryMethods
        {
            get => RawDeliveryMethods.ConvertAll(Enum.Parse<DeliveryMethod>);
            set => RawDeliveryMethods = value.ConvertAll(dm => dm.ToString());
        }
    }
}
