using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace EvidenceApi.V1.Gateways
{
    public class NotifyGateway
    {
        private INotificationClient _client;
        public NotifyGateway(INotificationClient client)
        {
            _client = client;
        }

        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason reason, Resident resident)
        {
            var result = Deliver(deliveryMethod, reason, resident);
        }

        private NotificationResponse Deliver(DeliveryMethod deliveryMethod, CommunicationReason reason, Resident resident)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Email => _client.SendEmail(resident.Email, GetEmailTemplateId(reason), null, null, null),
                DeliveryMethod.Sms => _client.SendSms(resident.PhoneNumber, GetSmsTemplateId(reason), null, null, null),
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod, $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }

        private static string GetSmsTemplateId(CommunicationReason reason)
        {
            return reason switch
            {
                CommunicationReason.Reminder => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_REMINDER_SMS"),
                CommunicationReason.EvidenceRejected => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_EVIDENCE_REJECTED_SMS"),
                CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS"),
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, $"Communication Reason {reason.ToString()} not recognised")
            };
        }


        private static string GetEmailTemplateId(CommunicationReason reason)
        {
            return reason switch
            {
                CommunicationReason.Reminder => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_REMINDER_EMAIL"),
                CommunicationReason.EvidenceRejected => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_EVIDENCE_REJECTED_EMAIL"),
                CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable("NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL"),
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, $"Communication Reason {reason.ToString()} not recognised")
            };
        }
    }
}
