using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using Notify.Exceptions;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase
{
    public class CreateEvidenceRequestUseCase : ICreateEvidenceRequestUseCase
    {
        private readonly IEvidenceRequestValidator _validator;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly INotifyGateway _notifyGateway;
        private readonly IFindOrCreateResidentReferenceIdUseCase _findOrCreateResidentReferenceIdUseCase;
        private readonly ILogger<CreateEvidenceRequestUseCase> _logger;

        public CreateEvidenceRequestUseCase(
            IEvidenceRequestValidator validator,
            IDocumentTypeGateway documentTypeGateway,
            IResidentsGateway residentsGateway,
            IEvidenceGateway evidenceGateway,
            INotifyGateway notifyGateway,
            IFindOrCreateResidentReferenceIdUseCase findOrCreateResidentReferenceIdUseCase,
            ILogger<CreateEvidenceRequestUseCase> logger)
        {
            _validator = validator;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
            _evidenceGateway = evidenceGateway;
            _notifyGateway = notifyGateway;
            _findOrCreateResidentReferenceIdUseCase = findOrCreateResidentReferenceIdUseCase;
            _logger = logger;
        }

        public EvidenceRequestResponse Execute(EvidenceRequestRequest request)
        {
            var validation = _validator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }

            var resident = _residentsGateway.FindOrCreateResident(BuildResident(request.Resident));
            var documentTypes = request.DocumentTypes.ConvertAll<DocumentType>(dt => _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(request.Team, dt));

            var residentReferenceId = _findOrCreateResidentReferenceIdUseCase.Execute(resident);
            var evidenceRequest = BuildEvidenceRequest(request, resident.Id, residentReferenceId);
            var created = _evidenceGateway.CreateEvidenceRequest(evidenceRequest);

            try
            {
                created.DeliveryMethods.ForEach(dm =>
                    _notifyGateway.SendNotification(dm, CommunicationReason.EvidenceRequest, created, resident));
            }
            catch (NotifyClientException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new NotificationException(created);
            }

            return created.ToResponse(resident, documentTypes);
        }

        private EvidenceRequest BuildEvidenceRequest(EvidenceRequestRequest request, Guid residentId, string residentReferenceId)
        {
            return new EvidenceRequest
            {
                DocumentTypes = request.DocumentTypes,
                DeliveryMethods = request.DeliveryMethods.ConvertAll(ParseDeliveryMethod),
                Team = request.Team,
                Reason = request.Reason,
                UserRequestedBy = request.UserRequestedBy,
                ResidentId = residentId,
                ResidentReferenceId = residentReferenceId
            };
        }

        private static Resident BuildResident(ResidentRequest request)
        {
            return new Resident
            {
                Email = request.Email,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber
            };
        }

        private DeliveryMethod ParseDeliveryMethod(string deliveryMethod)
        {
            return Enum.Parse<DeliveryMethod>(deliveryMethod, true);
        }
    }
}
