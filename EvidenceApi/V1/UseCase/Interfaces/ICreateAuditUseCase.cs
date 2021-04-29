namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateAuditUseCase
    {
        void Execute(string path, string method, string queryString, string hackneyToken);
    }
}
