using System;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Response
{
    public class MergeAndLinkResidentsResponse
    {
        public ResidentResponse Resident { get; set; }
        public Guid GroupId { get; set; }
    }
}
