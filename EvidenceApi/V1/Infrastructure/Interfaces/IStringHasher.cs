namespace EvidenceApi.V1.Infrastructure.Interfaces
{
    public interface IStringHasher
    {
        string create(string toHash);
    }
}
