using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IFindOrCreateResidentReferenceIdUseCase
    {
        string Execute(Resident resident);
    }
}
