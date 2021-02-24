using EvidenceApi.V1.Boundary.Response;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindDocumentSubmissionsByResidentIdUseCase
    {
        Task<List<DocumentSubmissionResponse>> ExecuteAsync(DocumentSubmissionSearchQuery request);
    }
}
