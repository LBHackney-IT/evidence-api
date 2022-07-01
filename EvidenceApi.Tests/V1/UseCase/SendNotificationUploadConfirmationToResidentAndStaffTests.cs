using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using System;
using Moq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FluentAssertions;
using EvidenceApi.V1.Domain.Enums;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using Notify.Exceptions;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class SendNotificationUploadConfirmationToResidentAndStaffTests
    {
        private SendNotificationUploadConfirmationToResidentAndStaff _classUnderTest;
        private Mock<INotifyGateway> _notifyGateway;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IResidentsGateway> _residentsGateway;
        private Mock<ILogger<SendNotificationUploadConfirmationToResidentAndStaff>> _logger;
        private Resident _resident;
        private EvidenceRequest _evidenceRequest;

        [SetUp]
        public void Setup()
        {
            _notifyGateway = new Mock<INotifyGateway>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _logger = new Mock<ILogger<SendNotificationUploadConfirmationToResidentAndStaff>>();
            _classUnderTest = new SendNotificationUploadConfirmationToResidentAndStaff(_notifyGateway.Object, _evidenceGateway.Object, _residentsGateway.Object, _logger.Object);
        }

        [Test]
        public void CanSendNotificationUploadConfirmationToResident()
        {
            SetupMocks();
            Action act = () => _classUnderTest.Execute(_evidenceRequest.Id);
            act.Should().NotThrow();
            _notifyGateway.Verify(x =>
               x.SendNotificationUploadConfirmationForResident(DeliveryMethod.Email, CommunicationReason.DocumentsUploadedResidentConfirmation, _evidenceRequest, _resident));

            _notifyGateway.Verify(x =>
                x.SendNotificationUploadConfirmationForResident(DeliveryMethod.Sms, CommunicationReason.DocumentsUploadedResidentConfirmation, _evidenceRequest, _resident));
        }

        [Test]
        public void CallsGatewaysUsingCorrectIds()
        {
            SetupMocks();

            _classUnderTest.Execute(_evidenceRequest.Id);

            _residentsGateway.Verify(x => x.FindResident(It.Is<Guid>(id => id == _resident.Id)));

            _evidenceGateway.Verify(x => x.FindEvidenceRequest(It.Is<Guid>(id => id == _evidenceRequest.Id)));
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenEvidenceRequestCannotBefound()
        {
            var id = Guid.Empty;
            Action act = () => _classUnderTest.Execute(id);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find evidence request with id: {id}");
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenResidentCannotBefound()
        {
            _evidenceRequest = TestDataHelper.EvidenceRequest();
            _evidenceGateway.Setup(x =>
                x.FindEvidenceRequest(It.IsAny<Guid>())).Returns(_evidenceRequest).Verifiable();
            Action act = () => _classUnderTest.Execute(_evidenceRequest.Id);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find resident with id: {_evidenceRequest.ResidentId}");
        }

        [Test]
        public void ThrowsNotifyClientExceptionWhenThereIsAGovNotifyError()
        {
            SetupMocks();
            _notifyGateway.Setup(x =>
                x.SendNotificationUploadConfirmationForResident(
                    It.IsAny<DeliveryMethod>(),
                    CommunicationReason.DocumentsUploadedResidentConfirmation,
                    _evidenceRequest,
                    _resident))
                .Throws(new NotifyClientException()).Verifiable();
            Action act = () => _classUnderTest.Execute(_evidenceRequest.Id);
            act.Should().Throw<NotifyClientException>();
        }

        private void SetupMocks()
        {
            _resident = TestDataHelper.Resident();
            _evidenceRequest = TestDataHelper.EvidenceRequest();
            _evidenceRequest.ResidentId = _resident.Id;
            _evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms };

            _residentsGateway.Setup(x => x.FindResident(_resident.Id)).Returns(_resident).Verifiable();
            _evidenceGateway.Setup(x => x.FindEvidenceRequest(_evidenceRequest.Id)).Returns(_evidenceRequest).Verifiable();
        }
    }
}
