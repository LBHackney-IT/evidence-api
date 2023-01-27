using System.Collections.Generic;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces;

public interface IBackfillClaimTableWithResidentGroupIdUseCase
{
    Task<List<ClaimBackfillResponse>> ExecuteAsync();
}
