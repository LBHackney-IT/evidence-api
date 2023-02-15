using System;
using EvidenceApi.V1.Domain.Enums;
namespace EvidenceApi.V1.Boundary.Request
{
    public class ResidentSearchQuery
    {
        public string Team { get; set; }
        public Guid? GroupId { get; set; }
        public string SearchQuery { get; set; }
    }
}
