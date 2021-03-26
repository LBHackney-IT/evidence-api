using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class Team
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<DocumentType> DocumentTypes { get; set; }
    }
}
