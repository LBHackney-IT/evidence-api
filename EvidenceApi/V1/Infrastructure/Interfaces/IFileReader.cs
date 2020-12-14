namespace EvidenceApi.V1.Infrastructure.Interfaces
{
    public interface IFileReader<T>
    {
        T GetData();
    }
}
