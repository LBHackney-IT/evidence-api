namespace EvidenceApi.V1.Infrastructure.Interfaces
{
    public interface IOptions
    {
        public string NotifyApiKey { get; }
        public string DocumentTypeConfigPath { get; }
        public string DatabaseConnectionString { get; }
    }
}
