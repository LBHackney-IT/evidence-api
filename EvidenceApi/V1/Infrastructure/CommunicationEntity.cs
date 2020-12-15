using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    [Table("communications")]
    public class CommunicationEntity : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("delivery_method")]
        public string DeliveryMethod { get; set; }

        [Column("notify_id")]
        public string NotifyId { get; set; }

        [Column("template_id")]
        public string TemplateId { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("evidence_request_id")]
        [ForeignKey("EvidenceRequest")]
        public Guid EvidenceRequestId { get; set; }

        [ForeignKey("EvidenceRequestId")]
        public virtual EvidenceRequestEntity EvidenceRequest { get; set; }
    }
}
