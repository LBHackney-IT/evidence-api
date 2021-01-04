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

namespace EvidenceApi.V1.UseCase
{
    public class CreateEvidenceRequestUseCase : ICreateEvidenceRequestUseCase
    {
        private readonly IEvidenceRequestValidator _validator;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly INotifyGateway _notifyGateway;

        public CreateEvidenceRequestUseCase(
            IEvidenceRequestValidator validator,
            IDocumentTypeGateway documentTypeGateway,
            IResidentsGateway residentsGateway,
            IEvidenceGateway evidenceGateway,
            INotifyGateway notifyGateway)
        {
            _validator = validator;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
            _evidenceGateway = evidenceGateway;
            _notifyGateway = notifyGateway;
        }

        public EvidenceRequestResponse Execute(EvidenceRequestRequest request)
        {
            var validation = _validator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }

            var resident = _residentsGateway.FindOrCreateResident(request.Resident);
            var documentTypes = request.DocumentTypes.ConvertAll(FindDocumentType);
            var evidenceRequest = CreateEvidenceRequestModel(request, resident.Id);
            var created = _evidenceGateway.CreateEvidenceRequest(evidenceRequest);

            try
            {
                created.DeliveryMethods.ForEach(dm =>
                    _notifyGateway.SendNotification(dm, CommunicationReason.EvidenceRequest, created, resident));
            }
            catch (NotifyClientException ex)
            {
                Console.Error.WriteLine(ex);
                throw new NotificationException(created);
            }

            return created.ToResponse(resident, documentTypes);
        }

        private EvidenceRequest CreateEvidenceRequestModel(EvidenceRequestRequest request, Guid residentId)
        {
            return new EvidenceRequest
            {
                DocumentTypeIds = request.DocumentTypes,
                DeliveryMethods = request.DeliveryMethods.ConvertAll(ParseDeliveryMethod),
                ServiceRequestedBy = request.ServiceRequestedBy,
                ResidentId = residentId
            };
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }

        private DeliveryMethod ParseDeliveryMethod(string deliveryMethod)
        {
            return Enum.Parse<DeliveryMethod>(deliveryMethod, true);
        }
    }
}
