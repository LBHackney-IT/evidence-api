using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class DocumentSubmissionQueryResponse
    {
        public List<DocumentSubmission> DocumentSubmissions { get; set; }
        public int Total { get; set; }
    }
}
