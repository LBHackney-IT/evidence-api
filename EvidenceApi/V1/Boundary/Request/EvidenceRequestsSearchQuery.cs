using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Request
{
    public class EvidenceRequestsSearchQuery
    {
        public string ServiceRequestedBy { get; set; }
        public Guid? ResidentId { get; set; } = null;
    }
}