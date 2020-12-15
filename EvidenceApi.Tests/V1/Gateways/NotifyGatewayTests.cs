using System;
using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways;
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
        private NotifyGateway _classUnderTest;
        private Mock<INotificationClient> _notifyClient;

        public NotifyGatewayTests()
        {
            _notifyClient = new Mock<INotificationClient>();
            _classUnderTest = new NotifyGateway(_notifyClient.Object);
        }

        [TestCase(CommunicationReason.EvidenceRequest, DeliveryMethod.Email, "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL")]
        [TestCase(CommunicationReason.EvidenceRejected, DeliveryMethod.Email, "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_EMAIL")]
        [TestCase(CommunicationReason.Reminder, DeliveryMethod.Email, "NOTIFY_TEMPLATE_REMINDER_EMAIL")]
        public void CanSendAnEmail(CommunicationReason reason, DeliveryMethod deliveryMethod, string envVar)
        {
            var resident = _fixture.Create<Resident>();
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient
                .Setup(x =>
                    x.SendEmail(resident.Email, expectedTemplateId, null, null, null))
                .Returns(response)
                .Verifiable();

            _classUnderTest.SendNotification(deliveryMethod, reason, resident);
            _notifyClient.Verify();
        }

        [TestCase(CommunicationReason.EvidenceRequest, DeliveryMethod.Sms, "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS")]
        [TestCase(CommunicationReason.EvidenceRejected, DeliveryMethod.Sms, "NOTIFY_TEMPLATE_EVIDENCE_REJECTED_SMS")]
        [TestCase(CommunicationReason.Reminder, DeliveryMethod.Sms, "NOTIFY_TEMPLATE_REMINDER_SMS")]
        public void CanSendAnSms(CommunicationReason reason, DeliveryMethod deliveryMethod, string envVar)
        {
            var resident = _fixture.Create<Resident>();
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            var response = _fixture.Create<SmsNotificationResponse>();
            _notifyClient
                .Setup(x =>
                    x.SendSms(resident.PhoneNumber, expectedTemplateId, null, null, null))
                .Returns(response)
                .Verifiable();

            _classUnderTest.SendNotification(deliveryMethod, reason, resident);
            _notifyClient.Verify();
        }

    }
}
