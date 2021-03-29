namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionUpdateRequest
    {
        public string State { get; set; }
        public string StaffSelectedDocumentTypeId { get; set; } = null;
        public string RejectionReason { get; set; } = null;
    }
}
