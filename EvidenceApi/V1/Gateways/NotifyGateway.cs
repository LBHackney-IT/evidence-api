using System;
using System.Collections.Generic;
using System.Linq;
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
            var formattedDocumentTypes = FormatDocumentTypes(evidenceRequest);
            var personalisation = GetParamsFor(communicationReason, evidenceRequest, resident, formattedDocumentTypes);
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

        public void SendNotificationEvidenceRejected(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, DocumentSubmission documentSubmission, Resident resident)
        {
            var personalisation = GetParamsForEvidenceRejected(communicationReason, documentSubmission, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, communicationReason);
            var result = Deliver(deliveryMethod, templateId, resident, personalisation);
            var communication = new Communication
            {
                DeliveryMethod = deliveryMethod,
                NotifyId = result.id,
                EvidenceRequestId = (Guid) documentSubmission.EvidenceRequestId,
                Reason = communicationReason,
                TemplateId = templateId
            };
            _evidenceGateway.CreateCommunication(communication);
        }

        public void SendNotificationDocumentUploaded(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident)
        {
            var personalisation = GetParamsForDocumentUploaded(communicationReason, evidenceRequest, resident);
            var templateId = GetTemplateIdFor(deliveryMethod, communicationReason);
            var result = DeliverEmail(templateId, evidenceRequest.NotificationEmail, personalisation);
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

        public void SendNotificationUploadConfirmationForResident(DeliveryMethod deliveryMethod, CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident)
        {
            var personalisation = GetParamsForUploadConfirmationForResident(communicationReason, evidenceRequest);
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

        private NotificationResponse Deliver(DeliveryMethod deliveryMethod, string templateId, Resident resident, Dictionary<string, object> personalisation)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.Email => _client.SendEmail(resident.Email, templateId, personalisation, null, null),
                DeliveryMethod.Sms => _client.SendSms(resident.PhoneNumber, templateId, personalisation, null, null),
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod, $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }

        private NotificationResponse DeliverEmail(string templateId, string emailAddress, Dictionary<string, object> personalisation)
        {
            return _client.SendEmail(emailAddress, templateId, personalisation, null, null);
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
                    CommunicationReason.DocumentsUploadedResidentConfirmation => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_DOCUMENTS_UPLOADED_RESIDENT_CONFIRMATION_SMS"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                DeliveryMethod.Email => communicationReason switch
                {
                    CommunicationReason.EvidenceRequest => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL"),
                    CommunicationReason.EvidenceRejected => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_EMAIL"),
                    CommunicationReason.DocumentUploaded => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_DOCUMENT_UPLOADED_EMAIL"),
                    CommunicationReason.DocumentsUploadedResidentConfirmation => Environment.GetEnvironmentVariable(
                        "NOTIFY_TEMPLATE_DOCUMENTS_UPLOADED_RESIDENT_CONFIRMATION_EMAIL"),
                    _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason,
                        $"Communication Reason {communicationReason.ToString()} not recognised")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod,
                    $"Delivery Method {deliveryMethod.ToString()} not recognised")

            };
        }


        private Dictionary<string, object> GetParamsFor(CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident, string documentTypes)
        {
            return communicationReason switch
            {
                CommunicationReason.EvidenceRequest => new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"reason", evidenceRequest.Reason},
                    {"magic_link", MagicLinkFor(evidenceRequest)},
                    {"note_to_resident", evidenceRequest.NoteToResident},
                    {"document_types", documentTypes}
                },
                _ => throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised")
            };
        }

        private Dictionary<string, object> GetParamsForDocumentUploaded(CommunicationReason communicationReason, EvidenceRequest evidenceRequest, Resident resident)
        {
            if (communicationReason == CommunicationReason.DocumentUploaded)
            {
                return new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"resident_page_link", ResidentPageLinkFor(evidenceRequest)}
                };
            }
            throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised");
        }

        // passing in the document submission so we can track back to the evidence request it belongs to
        private Dictionary<string, object> GetParamsForEvidenceRejected(CommunicationReason communicationReason, DocumentSubmission documentSubmission, Resident resident)
        {
            if (communicationReason == CommunicationReason.EvidenceRejected)
            {
                return new Dictionary<string, object>
                {
                    {"resident_name", resident.Name},
                    {"evidence_item", GetDocumentType(documentSubmission.EvidenceRequest, documentSubmission.DocumentTypeId).Title},
                    {"rejection_reason", documentSubmission.RejectionReason},
                    {"magic_link", MagicLinkFor(documentSubmission.EvidenceRequest)}
                };
            }
            throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised");
        }

        private static Dictionary<string, object> GetParamsForUploadConfirmationForResident(CommunicationReason communicationReason, EvidenceRequest evidenceRequest)
        {
            if (communicationReason == CommunicationReason.DocumentsUploadedResidentConfirmation)
            {
                return new Dictionary<string, object>
                {
                    {"team", evidenceRequest.Team},
                    {"ref", evidenceRequest.ResidentReferenceId}
                };
            }
            throw new ArgumentOutOfRangeException(nameof(communicationReason), communicationReason, $"Communication Reason {communicationReason.ToString()} not recognised");
        }

        private DocumentType GetDocumentType(EvidenceRequest evidenceRequest, string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeByTeamNameAndDocumentTypeId(evidenceRequest.Team, documentTypeId);
        }

        private string MagicLinkFor(EvidenceRequest evidenceRequest) => $"{_options.EvidenceRequestClientUrl}resident/{evidenceRequest.Id}";
        private string ResidentPageLinkFor(EvidenceRequest evidenceRequest) => $"{_options.EvidenceRequestClientUrl}teams/{_documentTypeGateway.GetTeamIdByTeamName(evidenceRequest.Team)}/dashboard/residents/{evidenceRequest.ResidentId}";

        private string FormatDocumentTypes(EvidenceRequest evidenceRequest)
        {
            var teamDocumentTypes = _documentTypeGateway.GetDocumentTypesByTeamName(evidenceRequest.Team);
            var evidenceRequestDocumentTypes = evidenceRequest.DocumentTypes;

            var documentTypeTitles = teamDocumentTypes.Where(tdt => evidenceRequestDocumentTypes.Contains(tdt.Id)).Select(dt => dt.Title);

            return string.Join(",\n", documentTypeTitles);
        }
    }
}
