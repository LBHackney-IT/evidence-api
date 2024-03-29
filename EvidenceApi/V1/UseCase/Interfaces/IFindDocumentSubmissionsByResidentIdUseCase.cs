using EvidenceApi.V1.Boundary.Response;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindDocumentSubmissionsByResidentIdUseCase
    {
        Task<DocumentSubmissionResponseObject> ExecuteAsync(DocumentSubmissionSearchQuery request);
    }
}
