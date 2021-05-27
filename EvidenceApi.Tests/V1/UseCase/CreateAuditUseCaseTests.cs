using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateAuditUseCaseTests
    {
        private CreateAuditUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private readonly IFixture _fixture = new Fixture();
        private AuditEventRequest _request;
        private AuditEvent _created;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new CreateAuditUseCase(_evidenceGateway.Object);

        }

        [Test]
        public void ReturnsTheCreatedAuditEvent()
        {
            SetupMocks();
            var result = _classUnderTest.Execute(_request);
            Console.WriteLine(_request.Path);
            Console.WriteLine(_created.UrlVisited);
            Console.WriteLine(result.UrlVisited);

            //result.Id.Should().NotBeEmpty();
            result.UrlVisited.Should().Be(_created.UrlVisited);
            result.UserEmail.Should().Be(_created.UserEmail);
            result.RequestBody.Should().Be(_created.RequestBody);
            result.HttpMethod.Should().Be(_created.HttpMethod);
        }

        [Test]
        public void CallsGatewaysUsingCorrectParameters()
        {
            SetupMocks();

            _classUnderTest.Execute(_request);

            _evidenceGateway.Verify(x => x.CreateAuditEvent(
                It.Is<AuditEvent>(e =>
                    e.UrlVisited == _request.Path &&
                    e.UserEmail == _request.UserEmail &&
                    e.HttpMethod == _request.Method &&
                    e.RequestBody == _request.Request
                )
            ));
        }

        private void SetupMocks()
        {
            _created = _fixture.Build<AuditEvent>().Create();
            _request = _fixture.Build<AuditEventRequest>().Create();

            _evidenceGateway.Setup(x => x.CreateAuditEvent(It.IsAny<AuditEvent>())).Returns(_created).Verifiable();
        }
    }
}
