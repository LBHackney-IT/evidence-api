using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace EvidenceApi.V1.Gateways
{
    public class NotifyGateway : INotifyGateway
    {
        private readonly INotificationClient _client;
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly AppOptions _options;

        public NotifyGateway(INotificationClient client, IEvidenceGateway evidenceGateway, AppOptions options)
        {
            _client = client;
            _evidenceGateway = evidenceGateway;
            _options = options;
        }

        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, EvidenceRequest request, Resident resident)
        {
            var personalisation = GetParamsFor(communicationReason, request, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, communicationReason);
            var result = Deliver(deliveryMethod, templateId, resident, personalisation);
            var communication = new Communication
            {
                DeliveryMethod = deliveryMethod,
                NotifyId = result.id,
                EvidenceRequestId = request.Id,
                Reason = communicationReason,
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

        private static string GetTemplateIdFor(DeliveryMethod deliveryMethod, CommunicationReason communicationReason)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Sms => communicationReason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                DeliveryMethod.Email => communicationReason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod,
                    $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }


        private Dictionary<string, object> GetParamsFor(CommunicationReason communicationReason, EvidenceRequest request, Resident resident)
        {
            return communicationReason switch
            {
                CommunicationReason.EvidenceRequest => new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"reason", request.Reason},
                    {"magic_link", MagicLinkFor(request)}
                },
                _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised")
            };
        }

        private string MagicLinkFor(EvidenceRequest request) => $"{_options.EvidenceRequestClientUrl}resident/{request.Id}";
    }
}
