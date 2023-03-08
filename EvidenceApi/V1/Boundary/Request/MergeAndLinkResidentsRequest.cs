using System;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Request
{
    public class MergeAndLinkResidentsRequest
    {
        public string Team { get; set; }
        public Guid GroupId { get; set; }
        public Resident NewResident { get; set; }
        public Guid[] ResidentsToDelete { get; set; }
    }
}
