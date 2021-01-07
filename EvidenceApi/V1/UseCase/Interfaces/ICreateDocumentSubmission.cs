using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateDocumentSubmissionUseCase
    {
        DocumentSubmissionResponse Execute(Guid evienceRequestId, DocumentSubmissionRequest request);
    }
}
