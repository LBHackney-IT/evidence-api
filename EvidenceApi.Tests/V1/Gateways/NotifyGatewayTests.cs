using AutoFixture;
using EvidenceApi.V1.Gateways;
using Moq;
using Notify.Interfaces;
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

        [Test]
        public void CanSendAnEmail()
        {
            // _notifyClient.Setup()
        }
    }
}
