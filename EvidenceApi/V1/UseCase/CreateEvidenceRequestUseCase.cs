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
using System.Collections.Generic;
using System.Linq;

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
                _logger.LogError(validation.ToString());
                throw new BadRequestException(validation);
            }

            var resident = _residentsGateway.FindOrCreateResident(BuildResident(request.Resident));
            _residentsGateway.AddResidentGroupId(resident.Id, request.Team);
            var documentTypes = request.DocumentTypes.ConvertAll<DocumentType>(dt => _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(request.Team, dt));

            var residentReferenceId = _findOrCreateResidentReferenceIdUseCase.Execute(resident);
            var evidenceRequest = BuildEvidenceRequest(request, resident.Id, residentReferenceId);
            var created = _evidenceGateway.CreateEvidenceRequest(evidenceRequest);

            var exceptions = new List<DeliveryMethod>();

            created.DeliveryMethods.ForEach(
                dm =>
                {
                    try
                    {
                        _notifyGateway.SendNotification(dm, CommunicationReason.EvidenceRequest, created, resident);
                    }
                    catch (NotifyClientException e)
                    {
                        _logger.LogError(e, e.Message);
                        exceptions.Add(dm);
                    }
                }
            );

            ThrowException(created.DeliveryMethods, exceptions);

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
                NotificationEmail = request.NotificationEmail,
                ResidentId = residentId,
                ResidentReferenceId = residentReferenceId,
                NoteToResident = String.IsNullOrEmpty(request.NoteToResident) ? "There is no additional information" : request.NoteToResident
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

        private static void ThrowException(List<DeliveryMethod> deliveryMethods, List<DeliveryMethod> exceptions)
        {
            var successfulDeliveryMethods = deliveryMethods.Except(exceptions).ToList();

            if (exceptions.Count != 0)
            {
                if (deliveryMethods.Count > 1)
                {
                    throw new NotificationException(
                        "There was an error sending the request by " +
                        String.Join(", ", exceptions).ToLower() +
                        "."
                    );
                }
                throw new NotificationException(
                    "There was an error sending the request by " +
                    String.Join(", ", exceptions).ToLower() +
                    "."
                );
            }
        }
    }
}
