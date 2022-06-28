using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using Moq;
using Notify.Interfaces;
using Notify.Models.Responses;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class NotifyGatewayTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly NotifyGateway _classUnderTest;
        private readonly Mock<INotificationClient> _notifyClient;
        private readonly Mock<IEvidenceGateway> _evidenceGateway;
        private readonly Mock<IDocumentTypeGateway> _documentTypeGateway;
        private readonly AppOptions _options;

        public NotifyGatewayTests()
        {
            _options = _fixture.Create<AppOptions>();
            _notifyClient = new Mock<INotificationClient>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypeGateway = new Mock<IDocumentTypeGateway>();
            _classUnderTest = new NotifyGateway(_notifyClient.Object, _evidenceGateway.Object, _documentTypeGateway.Object, _options);
        }

        [Test]
        public void CanSendAnEvidenceRequestedSms()
        {
            var deliveryMethod = DeliveryMethod.Sms;
            var communicationReason = CommunicationReason.EvidenceRequest;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS";
            var resident = _fixture.Create<Resident>();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.DocumentTypes = new List<string> { "proof-of-id", "repairs-photo" };
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.Reason = "some-reason";

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var formattedDocumentTypes = "Proof of ID,\nRepairs photo";
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"reason", evidenceRequest.Reason},
                {"magic_link", $"{_options.EvidenceRequestClientUrl}resident/{evidenceRequest.Id}"},
                {"note_to_resident", evidenceRequest.NoteToResident},
                {"document_types", formattedDocumentTypes}
            };

            _documentTypeGateway.Setup(x => x.GetDocumentTypesByTeamName(It.IsAny<string>()))
                .Returns(TestDataHelper.GetDistinctDocumentTypes);

            var response = _fixture.Create<SmsNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotification(deliveryMethod, communicationReason, evidenceRequest, resident);
            _notifyClient.Verify(x =>
                x.SendSms(resident.PhoneNumber, expectedTemplateId,
                    It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null), Times.Once);
        }

        [Test]
        public void CanSendAnEvidenceRequestedEmail()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.EvidenceRequest;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL";
            var resident = _fixture.Create<Resident>();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.DocumentTypes = new List<string> { "proof-of-id", "repairs-photo" };
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.Reason = "some-reason";

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            var formattedDocumentTypes = "Proof of ID,\nRepairs photo";
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"reason", evidenceRequest.Reason},
                {"magic_link", $"{_options.EvidenceRequestClientUrl}resident/{evidenceRequest.Id}"},
                {"note_to_resident", evidenceRequest.NoteToResident},
                {"document_types", formattedDocumentTypes}
            };

            _documentTypeGateway.Setup(x => x.GetDocumentTypesByTeamName(It.IsAny<string>()))
                .Returns(TestDataHelper.GetDistinctDocumentTypes);

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotification(deliveryMethod, communicationReason, evidenceRequest, resident);
            _notifyClient.Verify(x =>
                    x.SendEmail(resident.Email, expectedTemplateId,
                        It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null), Times.Once);
        }

        [Test]
        public void CanSendAnUpdatedDocumentSubmissionSms()
        {
            SetupMocks();
            var deliveryMethod = DeliveryMethod.Sms;
            var communicationReason = CommunicationReason.EvidenceRejected;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_SMS";
            var resident = _fixture.Create<Resident>();

            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            documentSubmission.EvidenceRequest.ResidentId = resident.Id;
            documentSubmission.RejectionReason = "This is the rejection reason";

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"evidence_item", "Passport"},
                {"rejection_reason", documentSubmission.RejectionReason},
                {"magic_link", $"{_options.EvidenceRequestClientUrl}resident/{documentSubmission.EvidenceRequest.Id}"}
            };

            var response = _fixture.Create<SmsNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotificationEvidenceRejected(deliveryMethod, communicationReason, documentSubmission, resident);
            _notifyClient.Verify(x =>
                x.SendSms(resident.PhoneNumber, expectedTemplateId,
                    It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CanSendAnUpdatedDocumentSubmissionEmail()
        {
            SetupMocks();
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.EvidenceRejected;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_EMAIL";
            var resident = _fixture.Create<Resident>();

            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            documentSubmission.EvidenceRequest.ResidentId = resident.Id;
            documentSubmission.RejectionReason = "This is the rejection reason";

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"evidence_item", "Passport"},
                {"rejection_reason", documentSubmission.RejectionReason},
                {"magic_link", $"{_options.EvidenceRequestClientUrl}resident/{documentSubmission.EvidenceRequest.Id}"}
            };

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotificationEvidenceRejected(deliveryMethod, communicationReason, documentSubmission, resident);
            _notifyClient.Verify(x =>
                x.SendEmail(resident.Email, expectedTemplateId,
                    It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CanSendADocumentUploadedEmail()
        {
            SetupMocks();
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.DocumentUploaded;
            var envVar = "NOTIFY_TEMPLATE_DOCUMENT_UPLOADED_EMAIL";
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            evidenceRequest.NotificationEmail = "some@email";
            evidenceRequest.Reason = "some-reason";
            evidenceRequest.ResidentId = resident.Id;

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"resident_page_link", $"{_options.EvidenceRequestClientUrl}teams/2/dashboard/residents/{evidenceRequest.ResidentId}"}
            };

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotificationDocumentUploaded(deliveryMethod, communicationReason, evidenceRequest, resident);
            _notifyClient.Verify(x =>
                    x.SendEmail(evidenceRequest.NotificationEmail, expectedTemplateId,
                        It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CanSendADocumentsUploadedResidentConfirmationEmail()
        {
            SetupMocks();
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.DocumentsUploadedResidentConfirmation;
            var envVar = "NOTIFY_TEMPLATE_DOCUMENTS_UPLOADED_RESIDENT_CONFIRMATION_EMAIL";
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            evidenceRequest.Team = "some-team";
            evidenceRequest.ResidentReferenceId = "AB123456";
            evidenceRequest.ResidentId = resident.Id;

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"team", evidenceRequest.Team},
                {"ref", evidenceRequest.ResidentReferenceId}
            };

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotificationUploadConfirmationForResident(deliveryMethod, communicationReason, evidenceRequest, resident);
            _notifyClient.Verify(x =>
                    x.SendEmail(resident.Email, expectedTemplateId,
                        It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CanSendADocumentsUploadedResidentConfirmationSms()
        {
            SetupMocks();
            var deliveryMethod = DeliveryMethod.Sms;
            var communicationReason = CommunicationReason.DocumentsUploadedResidentConfirmation;
            var envVar = "NOTIFY_TEMPLATE_DOCUMENTS_UPLOADED_RESIDENT_CONFIRMATION_SMS";
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            evidenceRequest.Team = "some-team";
            evidenceRequest.ResidentReferenceId = "AB123456";
            evidenceRequest.ResidentId = resident.Id;

            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"team", evidenceRequest.Team},
                {"ref", evidenceRequest.ResidentReferenceId}
            };

            var response = _fixture.Create<SmsNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotificationUploadConfirmationForResident(deliveryMethod, communicationReason, evidenceRequest, resident);
            _notifyClient.Verify(x =>
                    x.SendSms(resident.PhoneNumber, expectedTemplateId,
                        It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CreatesACommunicationForEvidenceRequested()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.EvidenceRequest;
            var resident = _fixture.Create<Resident>();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.DocumentTypes = new List<string> { "proof-of-id", "repairs-photo" };
            evidenceRequest.Reason = "some-reason";

            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL";
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            _documentTypeGateway.Setup(x => x.GetDocumentTypesByTeamName(It.IsAny<string>()))
                .Returns(TestDataHelper.GetDistinctDocumentTypes);

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.Setup(x =>
                    x.SendEmail(resident.Email, expectedTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null,
                        null))
                .Returns(response);

            _classUnderTest.SendNotification(deliveryMethod, communicationReason, evidenceRequest, resident);

            _evidenceGateway.Verify(x => x.CreateCommunication(It.Is<Communication>(x =>
                x.Reason == communicationReason && x.DeliveryMethod == deliveryMethod && x.TemplateId == expectedTemplateId &&
                x.NotifyId == response.id)));
        }

        [Test]
        public void CreatesACommunicationForDocumentUploaded()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.DocumentUploaded;
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            evidenceRequest.Reason = "some-reason";
            evidenceRequest.NotificationEmail = "some@email";
            evidenceRequest.ResidentId = resident.Id;

            var envVar = "NOTIFY_TEMPLATE_DOCUMENT_UPLOADED_EMAIL";
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.Setup(x =>
                    x.SendEmail(evidenceRequest.NotificationEmail, expectedTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null,
                        null))
                .Returns(response);

            _classUnderTest.SendNotificationDocumentUploaded(deliveryMethod, communicationReason, evidenceRequest, resident);

            _evidenceGateway.Verify(x => x.CreateCommunication(It.Is<Communication>(x =>
                x.Reason == communicationReason && x.DeliveryMethod == deliveryMethod && x.TemplateId == expectedTemplateId &&
                x.NotifyId == response.id)));
        }

        [Test]
        public void CreatesACommunicationForDocumentsUploadedResidentConfirmation()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var communicationReason = CommunicationReason.DocumentsUploadedResidentConfirmation;
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            evidenceRequest.Reason = "some-reason";
            evidenceRequest.ResidentId = resident.Id;

            var envVar = "NOTIFY_TEMPLATE_DOCUMENTS_UPLOADED_RESIDENT_CONFIRMATION_EMAIL";
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"team", evidenceRequest.Team},
                {"ref", evidenceRequest.ResidentReferenceId}
            };

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.Setup(x =>
                    x.SendEmail(resident.Email, expectedTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null,
                        null))
                .Returns(response);
            _classUnderTest.SendNotificationUploadConfirmationForResident(deliveryMethod, communicationReason, evidenceRequest, resident);
            _evidenceGateway.Verify(x => x.CreateCommunication(It.Is<Communication>(x =>
                x.Reason == communicationReason && x.DeliveryMethod == deliveryMethod && x.TemplateId == expectedTemplateId &&
                x.NotifyId == response.id)));
        }

        private static bool CompareDictionaries(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            return a.Keys.All(k => b[k].Equals(a[k]));
        }

        private void SetupMocks()
        {
            _documentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(TestDataHelper.DocumentType("passport-scan"));
            _documentTypeGateway.Setup(x => x.GetTeamIdByTeamName(It.IsAny<string>()))
                .Returns("2");
        }
    }
}
