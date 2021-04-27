namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateAuditUseCase
    {
        void Execute(string path, string hackneyToken);
    }
}
