using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindDocumentSubmissionByIdUseCase
    {
        DocumentSubmissionResponse Execute(Guid id);
    }
}
