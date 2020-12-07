namespace EvidenceApi.V1.Infrastructure
{
    public interface IFileReader<T>
    {
        T GetData();
    }
}
