using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Request
{
    public class ResidentRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
