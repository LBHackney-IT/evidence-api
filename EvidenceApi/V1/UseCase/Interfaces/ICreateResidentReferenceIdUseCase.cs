using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateResidentReferenceIdUseCase
    {
        string Execute(Resident resident);
    }
}
