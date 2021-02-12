using System;
using EvidenceApi.V1.Boundary.Response;
using System.Threading.Tasks;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindDocumentSubmissionByIdUseCase
    {
        Task<DocumentSubmissionResponse> ExecuteAsync(Guid id);
    }
}
