#nullable enable annotations
using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Boundary.Response
{
    public class EvidenceRequestResponse
    {
        public ResidentResponse Resident { get; set; }
        public List<string> DeliveryMethods { get; set; }
        public List<DocumentType> DocumentTypes { get; set; }
        public string ServiceRequestedBy { get; set; }
        public string Reason { get; set; }
        public string UserRequestedBy { get; set; }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DocumentSubmission? DocumentSubmission { get; set; }
    }
}
