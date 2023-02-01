using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class PaginatedClaimResponse
    {
        public List<Claim> Claims { get; set; }
        public Paging Paging { get; set; }
    }

    public class Paging
    {
        public Cursors Cursors { get; set; } = null;
        public bool HasBefore { get; set; }
        public bool HasAfter { get; set; }
    }

    public class Cursors
    {
        public string Before { get; set; } = null;
        public string After { get; set; } = null;
    }
}
