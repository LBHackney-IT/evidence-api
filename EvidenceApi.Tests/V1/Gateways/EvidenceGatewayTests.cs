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
            var expected = ExpectedEvidenceRequestsWithResidentIdAndState(request);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceAndResidentId()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Residents.Add(resident);

            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging",
                ResidentId = resident.Id
            };
            var expected = ExpectedEvidenceRequestsWithResidentId(request);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceOnly()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging"
            };
            var expected = ExpectedEvidenceRequestsWithService(request);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetEvidenceRequestsReturnsEmptyList()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "invalid-service"
            };
            ExpectedEvidenceRequestsForEmptyList();

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEmpty();
        }

        [Test]
        public void FindByResidentIdReturnsDocumentSubmissions()
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.EvidenceRequest = evidenceRequest1;
            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.EvidenceRequest = evidenceRequest1;
            var documentSubmission3 = TestDataHelper.DocumentSubmission();
            documentSubmission3.EvidenceRequest = evidenceRequest2;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission3);
            DatabaseContext.SaveChanges();

            var expectedDocumentSubmissions = new List<DocumentSubmission>()
            {
                documentSubmission1, documentSubmission2
            };

            var found = _classUnderTest.FindDocumentSubmissionByEvidenceRequestId(evidenceRequest1.Id);

            found.Should().BeEquivalentTo(expectedDocumentSubmissions);
        }

        [Test]
        public void FindByResidentIdReturnsEmptyListWhenDocumentSubmissionsCannotBeFound()
        {
            var id = Guid.NewGuid();
            var found = _classUnderTest.FindDocumentSubmissionByEvidenceRequestId(id);
            found.Should().BeEmpty();
        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithResidentIdAndState(EvidenceRequestsSearchQuery request)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();

            var resident1 = TestDataHelper.Resident();
            var resident2 = TestDataHelper.Resident();

            evidenceRequest1.ServiceRequestedBy = "some-service";
            evidenceRequest2.ServiceRequestedBy = "some-other-service";
            evidenceRequest3.ServiceRequestedBy = request.ServiceRequestedBy;

            evidenceRequest1.ResidentId = resident1.Id;
            evidenceRequest2.ResidentId = resident2.Id;
            evidenceRequest3.ResidentId = (Guid) request.ResidentId;

            evidenceRequest1.State = EvidenceRequestState.Pending;
            evidenceRequest2.State = EvidenceRequestState.ForReview;
            evidenceRequest3.State = (EvidenceRequestState) request.State;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest3);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest3);
            return expected;
        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithService(EvidenceRequestsSearchQuery request)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();

            evidenceRequest1.ServiceRequestedBy = "some-other-service";
            evidenceRequest2.ServiceRequestedBy = request.ServiceRequestedBy;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest2);
            return expected;
        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithResidentId(EvidenceRequestsSearchQuery request)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();

            var resident1 = TestDataHelper.Resident();

            evidenceRequest1.ServiceRequestedBy = "some-other-service";
            evidenceRequest2.ServiceRequestedBy = request.ServiceRequestedBy;

            evidenceRequest1.ResidentId = resident1.Id;
            evidenceRequest2.ResidentId = (Guid) request.ResidentId;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest2);
            return expected;
        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithState()
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();

            evidenceRequest1.ServiceRequestedBy = "some-other-service";
            evidenceRequest2.ServiceRequestedBy = "development-team-staging";

            evidenceRequest1.State = EvidenceRequestState.Pending;
            evidenceRequest2.State = EvidenceRequestState.Approved;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest1);
            expected.Add(evidenceRequest2);
            return expected;
        }

        public void ExpectedEvidenceRequestsForEmptyList()
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();

            evidenceRequest1.ServiceRequestedBy = "some-other-service";
            evidenceRequest2.ServiceRequestedBy = "development-team-staging";

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();
        }
    }
}
