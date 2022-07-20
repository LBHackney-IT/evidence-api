using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using System;
using Microsoft.Extensions.Logging;
using Notify.Exceptions;


namespace EvidenceApi.V1.UseCase
{
    public class SendNotificationUploadConfirmationToResidentAndStaff : ISendNotificationUploadConfirmationToResidentAndStaff
    {
        private readonly INotifyGateway _notifyGateway;
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IResidentsGateway _residentsGateway;
        private readonly ILogger<SendNotificationUploadConfirmationToResidentAndStaff> _logger;

        public SendNotificationUploadConfirmationToResidentAndStaff(
            INotifyGateway notifyGateway,
            IEvidenceGateway evidenceGateway,
            IResidentsGateway residentsGateway,
            ILogger<SendNotificationUploadConfirmationToResidentAndStaff> logger)
        {
            _notifyGateway = notifyGateway;
            _evidenceGateway = evidenceGateway;
            _residentsGateway = residentsGateway;
            _logger = logger;
        }

        public void Execute(Guid evidenceRequestId)
        {
            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(evidenceRequestId);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find evidence request with id: {evidenceRequestId}");
            }

            var resident = _residentsGateway.FindResident(evidenceRequest.ResidentId);
            if (resident == null)
            {
                throw new NotFoundException($"Cannot find resident with id: {evidenceRequest.ResidentId}");
            }

            evidenceRequest.DeliveryMethods.ForEach(
                dm =>
                {
                    try
                    {
                        _notifyGateway.SendNotificationUploadConfirmationForResident(dm, CommunicationReason.DocumentsUploadedResidentConfirmation, evidenceRequest, resident);
                    }
                    catch (NotifyClientException e)
                    {
                        _logger.LogError(e, e.Message);
                        throw;
                    }
                }
            );
            _notifyGateway.SendNotificationDocumentUploaded(DeliveryMethod.Email, CommunicationReason.DocumentUploaded, evidenceRequest, resident);
        }
    }
}
