using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Gateways.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class CreateAuditUseCase : ICreateAuditUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;

        public CreateAuditUseCase(IEvidenceGateway evidenceGateway)
        {
            _evidenceGateway = evidenceGateway;
        }

        public AuditEvent Execute(AuditEventRequest request)
        {
            var auditEvent = BuildAuditEvent(request);
            var createdAuditEvent = _evidenceGateway.CreateAuditEvent(auditEvent);
            return createdAuditEvent;
        }

        private static AuditEvent BuildAuditEvent(AuditEventRequest request)
        {
            return new AuditEvent
            {
                UserEmail = request.UserEmail,
                UrlVisited = request.Path,
                HttpMethod = request.Method,
                RequestBody = request.Request
            };
        }
    }
}
