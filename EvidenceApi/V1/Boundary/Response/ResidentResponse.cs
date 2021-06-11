using System;

namespace EvidenceApi.V1.Boundary.Response
{
    public class ResidentResponse
    {
        public Guid Id { get; set; }
        public string ReferenceId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
