using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateEvidenceRequestUseCase
    {
        Task<EvidenceRequestResponse> ExecuteAsync(EvidenceRequestRequest request);
    }
}
