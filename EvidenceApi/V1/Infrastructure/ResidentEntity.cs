using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    [Table("resident")]
    public class ResidentEntity : IEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

    }
}
