using System;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IUpdateDocumentSubmissionVisibiltyUseCase
    {
        public void ExecuteAsync(Guid id, DocumentSubmissionVisibilityUpdateRequest request);
    }
}
