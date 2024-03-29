using System;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindResidentByIdUseCase
    {
        public ResidentResponse Execute(Guid id);
    }
}
