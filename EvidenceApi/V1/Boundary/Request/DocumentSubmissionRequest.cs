namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionRequest
    {
        public string Base64Document { get; set; }
        public string DocumentType { get; set; }
    }
}
