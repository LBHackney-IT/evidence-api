// TODO: This model should be replaced by an external data source

using System;

namespace EvidenceApi.V1.Domain
{
    public class Resident
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
