using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateResidentUseCase
    {
        ResidentResponse Execute(ResidentRequest request);
    }
}
