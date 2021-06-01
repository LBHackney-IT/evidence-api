using System;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Infrastructure.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace EvidenceApi.V1.Domain
{
    [SuppressMessage("ReSharper", "CA1056")]
    [Table("audit_events")]
    public class AuditEvent : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_email")]
        public string UserEmail { get; set; }

        [Column("url_visited")]
        public string UrlVisited { get; set; }

        [Column("http_method")]
        public string HttpMethod { get; set; }

        [Column("request_body")]
        public string RequestBody { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
