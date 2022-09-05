using System;

namespace EvidenceApi.V1.Domain
{
    public class DocumentType
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
    }
}
