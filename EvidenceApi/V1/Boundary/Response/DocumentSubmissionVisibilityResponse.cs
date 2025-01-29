
#nullable enable annotations

using System;
using System.Collections.Generic;
namespace EvidenceApi.V1.Boundary.Response
{
    public class DocumentSubmissionVisibilityResponse
    {
        public Guid Id { get; set; }
        public Guid? EvidenceRequestId { get; set; }
        public bool IsHidden { get; set; }

    }

    public class DocumentSubmissionVisibilityResponseObject
    {
        public List<DocumentSubmissionVisibilityResponse> DocumentSubmissions { get; set; }
        public int Total { get; set; }


    }
}
