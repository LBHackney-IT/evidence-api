using EvidenceApi.V1.Boundary.Response;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IGetEvidenceRequestsUseCase
    {
        List<EvidenceRequestResponse> Execute(EvidenceRequestsSearchQuery request);
    }
}
