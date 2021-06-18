using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceApi.V1.Boundary.Request
{
    public class DocumentSubmissionRequest
    {
        [FromRoute]
        public Guid EvidenceRequestId { get; set; }
        public IFormFile Image { get; set; }
        //public string DocumentType { get; set; }
        // public string ContentType { get; set; }
    }
}
