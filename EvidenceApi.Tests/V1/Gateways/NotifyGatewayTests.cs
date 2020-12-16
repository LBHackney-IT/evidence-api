using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Gateways.Interfaces;
using FluentAssertions;
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

        public NotifyGatewayTests()
        {
            _notifyClient = new Mock<INotificationClient>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new NotifyGateway(_notifyClient.Object, _evidenceGateway.Object);
        }

        [Test]
        public void CanSendAnEvidenceRequestedSms()
        {
            var deliveryMethod = DeliveryMethod.Sms;
            var reason = CommunicationReason.EvidenceRequest;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_SMS";
            var resident = _fixture.Create<Resident>();
            var request = _fixture.Build<EvidenceRequest>()
                .With(x => x.ResidentId, resident.Id)
                .Create();
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"service_name", request.ServiceRequestedBy},
                {"magic_link", request.MagicLink}
            };

            var response = _fixture.Create<SmsNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotification(deliveryMethod, reason, request, resident);
            _notifyClient.Verify(x =>
                x.SendSms(resident.PhoneNumber, expectedTemplateId,
                    It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CanSendAnEvidenceRequestedEmail()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var reason = CommunicationReason.EvidenceRequest;
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL";
            var resident = _fixture.Create<Resident>();
            var request = _fixture.Build<EvidenceRequest>()
                .With(x => x.ResidentId, resident.Id)
                .Create();
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);
            var expectedParams = new Dictionary<string, object>
            {
                {"resident_name", resident.Name},
                {"service_name", request.ServiceRequestedBy},
                {"magic_link", request.MagicLink}
            };

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.SetReturnsDefault(response);
            _classUnderTest.SendNotification(deliveryMethod, reason, request, resident);
            _notifyClient.Verify(x =>
                    x.SendEmail(resident.Email, expectedTemplateId,
                        It.Is<Dictionary<string, object>>(x => CompareDictionaries(expectedParams, x)), null, null));
        }

        [Test]
        public void CreatesACommunication()
        {
            var deliveryMethod = DeliveryMethod.Email;
            var reason = CommunicationReason.EvidenceRequest;
            var resident = _fixture.Create<Resident>();
            var request = _fixture.Build<EvidenceRequest>()
                .With(x => x.ResidentId, resident.Id)
                .Create();
            var envVar = "NOTIFY_TEMPLATE_EVIDENCE_REQUESTED_EMAIL";
            var expectedTemplateId = Environment.GetEnvironmentVariable(envVar);

            var response = _fixture.Create<EmailNotificationResponse>();
            _notifyClient.Setup(x =>
                    x.SendEmail(resident.Email, expectedTemplateId, It.IsAny<Dictionary<string, dynamic>>(), null,
                        null))
                .Returns(response);

            _classUnderTest.SendNotification(deliveryMethod, reason, request, resident);

            _evidenceGateway.Verify(x => x.CreateCommunication(It.Is<Communication>(x =>
                x.Reason == reason && x.DeliveryMethod == deliveryMethod && x.TemplateId == expectedTemplateId &&
                x.NotifyId == response.id)));
        }

        private static bool CompareDictionaries(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            // foreach (var kv in a)
            // {
            //     Console.WriteLine($"{kv.Key}: {a[kv.Key].ToString() == b[kv.Key].ToString()}");
            // }

            return a.Keys.All(k => b[k].ToString() == a[k].ToString());
        }

    }
}
