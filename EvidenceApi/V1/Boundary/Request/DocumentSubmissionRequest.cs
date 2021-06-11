namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionRequest
    {
        public string DocumentType { get; set; }
        public string State { get; set; }
        public string StaffSelectedDocumentTypeId { get; set; } = null;
    }
}
