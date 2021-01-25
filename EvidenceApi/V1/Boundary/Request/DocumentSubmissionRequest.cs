using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionRequest
    {
        public string DocumentType { get; set; }
        public string State { get; set; }


    }
}
