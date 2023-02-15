using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IAmendResidentGroupIdUseCase
    {
        Task<ResidentsTeamGroupId> Execute(ResidentGroupIdRequest request);
    }
}
