#nullable enable annotations
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
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class EvidenceGatewayTests : DatabaseTests
    {
        private EvidenceGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new EvidenceGateway(DatabaseContext);
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldInsertIntoTheDatabase()
        {
            var request = TestDataHelper.EvidenceRequest();
            var query = DatabaseContext.EvidenceRequests;

            _classUnderTest.CreateEvidenceRequest(request);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.DocumentTypes.Should().BeEquivalentTo(request.DocumentTypes);
            foundRecord.DeliveryMethods.Should().BeEquivalentTo(request.DeliveryMethods);
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldReturnTheCreatedRequest()
        {
            var request = TestDataHelper.EvidenceRequest();

            var created = _classUnderTest.CreateEvidenceRequest(request);
            var found = DatabaseContext.EvidenceRequests.First();

            created.Id.Should().Be(found.Id);
            created.CreatedAt.Should().Be(found.CreatedAt);

        }

        [Test]
        public void CreatingADocumentSubmissionShouldInsertIntoTheDatabase()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();

            var request = TestDataHelper.DocumentSubmission();
            request.EvidenceRequest = evidenceRequest;

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
            var request = TestDataHelper.DocumentSubmission(true);

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
            foundRecord.Reason.Should().Be(communication.Reason);
            foundRecord.DeliveryMethod.Should().Be(communication.DeliveryMethod);
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
            var request = TestDataHelper.EvidenceRequest();

            DatabaseContext.EvidenceRequests.Add(request);
            DatabaseContext.SaveChanges();

            var communication = TestDataHelper.Communication();
            communication.EvidenceRequest = request;
            return communication;
        }

        [Test]
        public void FindReturnsAnEvidenceRequest()
        {
            var dm = new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms };
            var expectedDeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Sms, DeliveryMethod.Email };

            var entity = TestDataHelper.EvidenceRequest();
            entity.DeliveryMethods = dm;

            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindEvidenceRequest(entity.Id);
            found.Id.Should().Be(entity.Id);
            found.CreatedAt.Should().Be(entity.CreatedAt);
            found.ResidentId.Should().Be(entity.ResidentId);
            found.DeliveryMethods.Should().BeEquivalentTo(expectedDeliveryMethods);
            found.DocumentTypes.Should().Equal(entity.DocumentTypes);
            found.ServiceRequestedBy.Should().Be(entity.ServiceRequestedBy);
        }

        [Test]
        public void FindReturnsNullWhenAnEvidenceRequestWithIdCannotBeFound()
        {
            Guid id = new Guid("7bb69c97-5e5a-48a5-ad40-e1563a1a7e53");
            var found = _classUnderTest.FindEvidenceRequest(id);
            found.Should().BeNull();
        }

        [Test]
        public void FindReturnsADocumentSubmission()
        {
            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindDocumentSubmission(documentSubmission.Id);

            found.Should().Be(documentSubmission);
        }

        [Test]
        public void FindReturnsNullWhenADocumentSubmissionWithIdCannotBeFound()
        {
            Guid id = Guid.NewGuid();
            var found = _classUnderTest.FindDocumentSubmission(id);
            found.Should().BeNull();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceAndResidentIdAndState()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging",
                ResidentId = resident.Id,
                State = EvidenceRequestState.Approved
            };
            var expected = ExpectedEvidenceRequests(resident, EvidenceRequestState.Approved);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceAndResidentId()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Residents.Add(resident);

            var expected = ExpectedEvidenceRequests(resident);
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging",
                ResidentId = resident.Id
            };

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceOnly()
        {
            var expected = ExpectedEvidenceRequests();
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging"
            };

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetEvidenceRequestsReturnsEmptyList()
        {
            var expected = ExpectedEvidenceRequests();
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "invalid-service"
            };

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEmpty();
        }

        public List<EvidenceRequest> ExpectedEvidenceRequests(Resident? resident = null, EvidenceRequestState? state = null)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();

            if (resident != null)
            {
                evidenceRequest1.ResidentId = resident.Id;
                evidenceRequest2.ResidentId = resident.Id;
            }

            if (state != null)
            {
                evidenceRequest1.State = (EvidenceRequestState) state;
                evidenceRequest2.State = (EvidenceRequestState) state;
            }

            evidenceRequest1.ServiceRequestedBy = "development-team-staging";
            evidenceRequest2.ServiceRequestedBy = "development-team-staging";

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest1);
            expected.Add(evidenceRequest2);
            return expected;
        }
    }
}
