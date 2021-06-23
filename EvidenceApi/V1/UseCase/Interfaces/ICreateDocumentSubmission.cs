using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using System.Threading.Tasks;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateDocumentSubmissionUseCase
    {
        Task<DocumentSubmissionResponse> ExecuteAsync(Guid evidenceRequestId, DocumentSubmissionRequest request);
    }
}
