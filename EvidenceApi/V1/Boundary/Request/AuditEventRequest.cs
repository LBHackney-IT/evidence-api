using System;

namespace EvidenceApi.V1.Boundary.Request
{
    public class AuditEventRequest
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string QueryString { get; set; }
        public string UserEmail { get; set; }
    }
}
