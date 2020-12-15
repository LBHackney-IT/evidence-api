using System;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class EvidenceGatewayTests : DatabaseTests
    {
        private readonly IFixture _fixture = new Fixture();
        private EvidenceGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new EvidenceGateway(DatabaseContext);
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldInsertIntoTheDatabase()
        {
            var request = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();
            var query = DatabaseContext.EvidenceRequests;

            _classUnderTest.CreateEvidenceRequest(request);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.DocumentTypes.Should().BeEquivalentTo(request.DocumentTypeIds);
            foundRecord.DeliveryMethods.Should().BeEquivalentTo(request.DeliveryMethods.ConvertAll(x => x.ToString()));
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldReturnTheCreatedRequest()
        {
            var request = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var created = _classUnderTest.CreateEvidenceRequest(request);
            var found = DatabaseContext.EvidenceRequests.First();

            created.Id.Should().Be(found.Id);
            created.CreatedAt.Should().Be(found.CreatedAt);

        }

        [Test]
        public void CreatingACommunicationShouldInsertIntoTheDatabase()
        {
            var query = DatabaseContext.Communications;
            var communication = CreateCommunicationFixture();

            var created = _classUnderTest.CreateCommunication(communication);

            query.Count().Should().Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Reason.Should().Be(communication.Reason.ToString());
            foundRecord.DeliveryMethod.Should().Be(communication.DeliveryMethod.ToString());
            foundRecord.EvidenceRequest.Id.Should().Be(communication.EvidenceRequestId);

            created.Id.Should().Be(foundRecord.Id);
        }

        [Test]
        public void CreatingACommunicationDoesNotCreateAnEvidenceRequest()
        {
            var communication = CreateCommunicationFixture();
            DatabaseContext.EvidenceRequests.Count().Should().Be(1);

            _classUnderTest.CreateCommunication(communication);

            DatabaseContext.EvidenceRequests.Count().Should().Be(1);
        }

        private Communication CreateCommunicationFixture()
        {
            var request = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var evidenceRequestEntity = request.ToEntity();
            DatabaseContext.EvidenceRequests.Add(evidenceRequestEntity);
            DatabaseContext.SaveChanges();
            request.Id = evidenceRequestEntity.Id;
            request.CreatedAt = evidenceRequestEntity.CreatedAt;

            return _fixture.Build<Communication>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .With(x => x.EvidenceRequestId, request.Id)
                .Create();

        }
    }
}
