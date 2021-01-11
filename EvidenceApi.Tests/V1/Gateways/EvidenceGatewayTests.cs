using System;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using EvidenceApi.V1.Domain.Enums;

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
        public void CreatingADocumentSubmissionShouldInsertIntoTheDatabase()
        {
            var evidenceRequest = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var request = _fixture.Build<DocumentSubmission>()
                .With(x => x.EvidenceRequest, evidenceRequest)
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var query = DatabaseContext.DocumentSubmissions;

            _classUnderTest.CreateDocumentSubmission(request);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.EvidenceRequest.Id.Should().NotBeEmpty();
            foundRecord.EvidenceRequest.ServiceRequestedBy.Should().Be(request.EvidenceRequest.ServiceRequestedBy);
            foundRecord.EvidenceRequest.UserRequestedBy.Should().Be(request.EvidenceRequest.UserRequestedBy);
            foundRecord.ClaimId.Should().Be(request.ClaimId);
            foundRecord.RejectionReason.Should().Be(request.RejectionReason);
            foundRecord.State.Should().Be(request.State);
            foundRecord.DocumentTypeId.Should().Be(request.DocumentTypeId);
        }

        [Test]
        public void CreatingADocumentSubmissionShouldReturnTheCreatedRequest()
        {
            var request = _fixture.Build<DocumentSubmission>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var created = _classUnderTest.CreateDocumentSubmission(request);
            var found = DatabaseContext.DocumentSubmissions.First();

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

        [Test]
        public void FindReturnsAnEvidenceRequest()
        {
            List<string> dm = new List<string> { "Sms", "Email" };
            var expectedDeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Sms, DeliveryMethod.Email };

            var entity = _fixture.Build<EvidenceRequestEntity>()
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .With(x => x.DeliveryMethods, dm)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindEvidenceRequest(entity.Id);
            found.Id.Should().Be(entity.Id);
            found.CreatedAt.Should().Be(entity.CreatedAt);
            found.ResidentId.Should().Be(entity.ResidentId);
            found.DeliveryMethods.Should().BeEquivalentTo(expectedDeliveryMethods);
            found.DocumentTypeIds.Should().Equal(entity.DocumentTypes);
            found.ServiceRequestedBy.Should().Be(entity.ServiceRequestedBy);
        }

        [Test]
        public void FindDoesNotReturnAnEvidenceRequest()
        {
            Guid id = new Guid("7bb69c97-5e5a-48a5-ad40-e1563a1a7e53");
            var found = _classUnderTest.FindEvidenceRequest(id);
            found.Should().BeNull();
        }
    }
}
