using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class ResidentRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Team { get; set; }
        public Guid? GroupId => null;
    }
}
