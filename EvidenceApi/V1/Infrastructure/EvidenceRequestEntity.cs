using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    [Table("evidence_requests")]
    public class EvidenceRequestEntity : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("resident_id")]
        public Guid ResidentId { get; set; }

        [Column("delivery_methods")]
        public List<string> DeliveryMethods { get; set; }

        [Column("document_types")]
        public List<string> DocumentTypes { get; set; }

        [Column("service_requested_by")]
        public string ServiceRequestedBy { get; set; }

        public virtual ICollection<CommunicationEntity> Communications { get; set; }
    }
}
