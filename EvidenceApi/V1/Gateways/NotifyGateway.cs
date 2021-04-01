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
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly AppOptions _options;

        public NotifyGateway(INotificationClient client, IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway, AppOptions options)
        {
            _client = client;
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
            _options = options;
        }

        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident)
        {
            var personalisation = GetParamsFor(communicationReason, evidenceRequest, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, communicationReason);
            var result = Deliver(deliveryMethod, templateId, resident, personalisation);
            var communication = new Communication
            {
                DeliveryMethod = deliveryMethod,
                NotifyId = result.id,
                EvidenceRequestId = evidenceRequest.Id,
                Reason = communicationReason,
                TemplateId = templateId
            };
            _evidenceGateway.CreateCommunication(communication);
        }

        public void SendNotification(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, DocumentSubmission documentSubmission, Resident resident)
        {
            var personalisation = GetParamsFor(communicationReason, documentSubmission, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, communicationReason);
            var result = Deliver(deliveryMethod, templateId, resident, personalisation);
            var communication = new Communication
            {
                DeliveryMethod = deliveryMethod,
                NotifyId = result.id,
                EvidenceRequestId = documentSubmission.EvidenceRequestId,
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
                    CommunicationReason.EvidenceRejected => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_SMS"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                DeliveryMethod.Email => communicationReason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL"),
                    CommunicationReason.EvidenceRejected => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_EMAIL"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod,
                    $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }


        private Dictionary<string, object> GetParamsFor(CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident)
        {
            return communicationReason switch
            {
                CommunicationReason.EvidenceRequest => new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"reason", evidenceRequest.Reason},
                    {"magic_link", MagicLinkFor(evidenceRequest)}
                },
                _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised")
            };
        }

        // passing in the document submission so we can track back to the evidence request it belongs to
        private Dictionary<string, object> GetParamsFor(CommunicationReason communicationReason, DocumentSubmission documentSubmission, Resident resident)
        {
            return communicationReason switch
            {
                CommunicationReason.EvidenceRejected => new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"evidence_item", GetDocumentType(documentSubmission.EvidenceRequest).Title},
                    {"rejection_reason", documentSubmission.RejectionReason},
                    {"magic_link", MagicLinkFor(documentSubmission.EvidenceRequest)}
                },
                _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised")
            };
        }

        private DocumentType GetDocumentType(EvidenceRequest evidenceRequest)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(evidenceRequest.ServiceRequestedBy, evidenceRequest.DocumentTypes[0]);
        }

        private string MagicLinkFor(EvidenceRequest evidenceRequest) => $"{_options.EvidenceRequestClientUrl}resident/{evidenceRequest.Id}";
    }
}
