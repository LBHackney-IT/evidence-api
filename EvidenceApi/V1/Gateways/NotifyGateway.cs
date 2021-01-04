using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways.Interfaces;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace EvidenceApi.V1.Gateways
{
    public class NotifyGateway : INotifyGateway
    {
        private readonly INotificationClient _client;
        private readonly IEvidenceGateway _evidenceGateway;

        public NotifyGateway(INotificationClient client, IEvidenceGateway evidenceGateway)
        {
            _client = client;
            _evidenceGateway = evidenceGateway;
        }

        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason reason, EvidenceRequest request, Resident resident)
        {
            var personalisation = GetParamsFor(reason, request, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, reason);
            var result = Deliver(deliveryMethod, templateId, resident, personalisation);
            var communication = new Communication
            {
                DeliveryMethod = deliveryMethod,
                NotifyId = result.id,
                EvidenceRequestId = request.Id,
                Reason = reason,
                TemplateId = templateId
            };
            _evidenceGateway.CreateCommunication(communication);
        }

        private NotificationResponse Deliver(DeliveryMethod deliveryMethod, string templateId, Resident resident, Dictionary<string, object> personalisation)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Email => _client.SendEmail(resident.Email, templateId, personalisation, null, null),
                DeliveryMethod.Sms => _client.SendSms(resident.PhoneNumber, templateId, personalisation, null, null),
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod, $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }

        private static string GetTemplateIdFor(DeliveryMethod deliveryMethod, CommunicationReason reason)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Sms => reason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS"),
                    _ => throw new ArgumentOutOfRangeException(nameof(reason), reason,
                        $"Communication Reason {reason.ToString()} not recognised")
                },
                DeliveryMethod.Email => reason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL"),
                    _ => throw new ArgumentOutOfRangeException(nameof(reason), reason,
                        $"Communication Reason {reason.ToString()} not recognised")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod,
                    $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }


        private static Dictionary<string, object> GetParamsFor(CommunicationReason reason, EvidenceRequest request, Resident resident)
        {
            return reason switch
            {
                CommunicationReason.EvidenceRequest => new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"service_name", request.ServiceRequestedBy},
                    {"magic_link", request.MagicLink}
                },
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, $"Communication Reason {reason.ToString()} not recognised")
            };
        }
    }
}
