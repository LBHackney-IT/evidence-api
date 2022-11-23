using System;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionSearchQuery
    {
        public string Team { get; set; }
        public Guid ResidentId { get; set; }
        public SubmissionState? State { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
