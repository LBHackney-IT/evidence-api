using System;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Domain
{
    public class Communication
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid EvidenceRequestId { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public string NotifyId { get; set; }
        public CommunicationReason Reason { get; set; }
        public string TemplateId { get; set; }
    }
}
