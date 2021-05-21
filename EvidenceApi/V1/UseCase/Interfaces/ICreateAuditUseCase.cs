using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Boundary.Request;
namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ICreateAuditUseCase
    {
        AuditEvent Execute(AuditEventRequest request);
    }
}