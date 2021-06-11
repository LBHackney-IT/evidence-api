namespace EvidenceApi.V1.Gateways.Interfaces
{
    // Did this because registering multiple implementations of the same interface seems to be a pain.
    // See https://stackoverflow.com/questions/39174989/how-to-register-multiple-implementations-of-the-same-interface-in-asp-net-core
    // This avoids needing to over complicate Startup.cs
    public interface IStaffSelectedDocumentTypeGateway : IDocumentTypeGateway
    {
    }
}
