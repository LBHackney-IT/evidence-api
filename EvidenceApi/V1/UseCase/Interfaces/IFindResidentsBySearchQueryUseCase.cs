using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindResidentsBySearchQueryUseCase
    {
        List<ResidentResponse> Execute(ResidentSearchQuery request);
    }
}
