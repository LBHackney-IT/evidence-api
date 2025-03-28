#nullable enable annotations
using System;
using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Boundary.Request;
using AutoFixture;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class EvidenceGatewayTests : DatabaseTests
    {
        private EvidenceGateway _classUnderTest;
        private static Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new EvidenceGateway(DatabaseContext);
        }

        [Test]
        public void CreatingAnAuditEventShouldInsertIntoTheDatabase()
        {
            var request = _fixture.Build<AuditEvent>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();
            var query = DatabaseContext.AuditEvents;

            _classUnderTest.CreateAuditEvent(request);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.UserEmail.Should().BeEquivalentTo(request.UserEmail);
            foundRecord.UrlVisited.Should().BeEquivalentTo(request.UrlVisited);
            foundRecord.HttpMethod.Should().BeEquivalentTo(request.HttpMethod);
            foundRecord.RequestBody.Should().BeEquivalentTo(request.RequestBody);
        }

        [Test]
        public void CreatingAnAuditEventShouldReturnTheCreatedEvent()
        {
            var request = _fixture.Build<AuditEvent>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            var created = _classUnderTest.CreateAuditEvent(request);
            var found = DatabaseContext.AuditEvents.First();

            created.Id.Should().Be(found.Id);
            created.CreatedAt.Should().Be(found.CreatedAt);
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
            foundRecord.EvidenceRequest.Team.Should().Be(request.EvidenceRequest.Team);
            foundRecord.EvidenceRequest.Reason.Should().Be(request.EvidenceRequest.Reason);
            foundRecord.EvidenceRequest.UserRequestedBy.Should().Be(request.EvidenceRequest.UserRequestedBy);
            foundRecord.ClaimId.Should().Be(request.ClaimId);
            foundRecord.RejectionReason.Should().Be(request.RejectionReason);
            foundRecord.State.Should().Be(request.State);
            foundRecord.DocumentTypeId.Should().Be(request.DocumentTypeId);
            foundRecord.IsHidden.Should().Be(false);
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
            found.ResidentReferenceId.Should().Be(entity.ResidentReferenceId);
            found.DeliveryMethods.Should().BeEquivalentTo(expectedDeliveryMethods);
            found.DocumentTypes.Should().Equal(entity.DocumentTypes);
            found.Team.Should().Be(entity.Team);
            found.Reason.Should().Be(entity.Reason);
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
        public void FindReturnsNullWhenTheDocumentSubmissionShouldBeHidden()
        {
            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            documentSubmission.IsHidden = true;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            var found = _classUnderTest.FindDocumentSubmission(documentSubmission.Id);

            found.Should().Be(null);
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceAndResidentIdAndState()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "development-team-staging",
                ResidentId = resident.Id,
                State = EvidenceRequestState.Approved
            };
            var expected = ExpectedEvidenceRequestsWithResidentIdAndState(request);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void WillReturnEmptyListIfOnlyTeamIsPassed()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "development-team-staging",
                ResidentId = null,
                State = null
            };
            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEmpty();
        }

        [Test]
        public void CanGetEvidenceRequestsByServiceAndResidentId()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Residents.Add(resident);

            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "development-team-staging",
                ResidentId = resident.Id
            };
            var expected = ExpectedEvidenceRequestsWithResidentId(request);

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEquivalentTo(expected);
            request.ResidentId.Should().NotBeEmpty();
        }

        [Test]
        public void GetEvidenceRequestsReturnsEmptyList()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "invalid-service"
            };
            ExpectedEvidenceRequestsForEmptyList();

            var result = _classUnderTest.GetEvidenceRequests(request);
            result.Should().BeEmpty();
        }

        [Test]
        public void FindEvidenceRequestsByResidentIdReturnsResults()
        {
            // Arrange
            var currentDate = new DateTime();
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            Thread.Sleep(1000);
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            Thread.Sleep(1000);
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();
            Thread.Sleep(1000);
            var resident1 = TestDataHelper.Resident();
            resident1.Id = Guid.NewGuid();
            var resident2 = TestDataHelper.Resident();
            resident2.Id = Guid.NewGuid();
            evidenceRequest1.ResidentId = resident1.Id;
            evidenceRequest1.CreatedAt = currentDate.AddDays(1);
            evidenceRequest2.ResidentId = resident1.Id;
            evidenceRequest2.CreatedAt = currentDate.AddDays(2);
            evidenceRequest3.ResidentId = resident2.Id;
            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest3);
            DatabaseContext.SaveChanges();
            var expected = new List<EvidenceRequest>()
            {
                evidenceRequest2, evidenceRequest1
            };

            // Act
            var found = _classUnderTest.FindEvidenceRequestsByResidentId(resident1.Id);

            // Assert
            found.Should().HaveCount(2);
            found.Should().Equal(expected);
        }

        [Test]
        public void GetAllReturnsResults()
        {
            // Arrange
            var currentDate = new DateTime();
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.CreatedAt = currentDate.AddDays(1);
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.CreatedAt = currentDate.AddDays(2);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.SaveChanges();
            var expected = new List<EvidenceRequest>()
            {
                evidenceRequest2, evidenceRequest1
            };

            // Act
            var found = _classUnderTest.GetAll();

            // Assert
            found.Should().Equal(expected);
        }

        [Test]
        public void CanGetEvidenceRequestsByTeamAndResidentReferenceId()
        {
            // Arrange
            var team = "Development Housing Team";
            var residentReferenceId = "12345";
            var request = new ResidentSearchQuery { Team = team, SearchQuery = residentReferenceId };

            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = team;
            evidenceRequest1.ResidentReferenceId = residentReferenceId;
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest2.Team = team;
            evidenceRequest2.ResidentReferenceId = "residentReferenceId";
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();


            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();
            var expected = new List<EvidenceRequest>()
            {
                evidenceRequest1
            };

            // Act
            var found = _classUnderTest.GetEvidenceRequests(request);

            // Assert
            found.Should().Equal(expected);

        }

        [Test]
        public void CanGetEvidenceRequestsWithDocumentSubmissionsInDescendingOrder()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "development-team-staging",
                ResidentId = Guid.NewGuid(),
            };
            var expected = ExpectedEvidenceRequestsWithDocumentSubmissions(request);

            var result = _classUnderTest.GetEvidenceRequestsWithDocumentSubmissions(request);
            result.Should().BeEquivalentTo(expected);
            result.Should().BeInDescendingOrder(r => r.CreatedAt);
            result[0].DocumentSubmissions.Should().BeInDescendingOrder(ds => ds.CreatedAt);
        }

        [Ignore("'Flaky test'")]
        [Test]
        public void GetDocumentSubmissionsByResidentIdReturnsAListOfDocumentSubmissionsWithDefaultPagination()
        {
            var queryGuid = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(queryGuid);
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "testTeam";

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission1.Team = team;
            var documentSubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission2.Team = team;

            DatabaseContext.Entry(documentSubmission1).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.Entry(documentSubmission2).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);

            DatabaseContext.SaveChanges();

            var expected = new List<DocumentSubmission>() { documentSubmission1, documentSubmission2 };

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(queryGuid, team);

            result.Total.Should().Be(2);
            result.DocumentSubmissions.Should().Equal(expected);
        }

        [Test]
        public void GetDocumentSubmissionsByResidentIdReturnsAListOfPaginatedDocuments()
        {
            var queryGuid = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(queryGuid);
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var page = 1;
            var pageSize = 2;
            var team = "testTeam";
            var date = new DateTime();

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission1.Team = team;
            documentSubmission1.CreatedAt = date.AddHours(1);
            var documentSubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission2.Team = team;
            documentSubmission2.CreatedAt = date.AddHours(2);
            var documentSubmission3 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission3.Team = team;
            documentSubmission3.CreatedAt = date.AddHours(3);
            var documentSubmission4 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission4.Team = team;
            documentSubmission4.CreatedAt = date.AddHours(4);

            DatabaseContext.Entry(documentSubmission1).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);

            DatabaseContext.Entry(documentSubmission2).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);

            DatabaseContext.Entry(documentSubmission3).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission3);

            DatabaseContext.Entry(documentSubmission4).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission4);

            DatabaseContext.SaveChanges();

            var expected = new List<DocumentSubmission>() { documentSubmission4, documentSubmission3 };

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(queryGuid, team, null, pageSize, page);
            result.Total.Should().Be(4);
            result.DocumentSubmissions.Should().Equal(expected);
        }

        [Test]
        public void GetDocumentSubmissionsByResidentIAndStatedReturnsAListOfPaginatedDocuments()
        {
            var queryGuid = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(queryGuid);
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "testTeam";
            var page = 1;
            var pageSize = 2;
            var currentDate = new DateTime();

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission1.Team = team;
            Thread.Sleep(1000);
            var documentSubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission2.Team = team;
            Thread.Sleep(1000);
            var documentSubmission3 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission3.Team = team;
            Thread.Sleep(1000);

            var documentSubmission4 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission4.Team = team;
            documentSubmission1.State = SubmissionState.Approved;
            documentSubmission2.State = SubmissionState.Pending;
            documentSubmission3.State = SubmissionState.Approved;
            documentSubmission4.State = SubmissionState.Approved;

            documentSubmission1.CreatedAt = currentDate.AddDays(1);
            documentSubmission2.CreatedAt = currentDate.AddDays(2);
            documentSubmission3.CreatedAt = currentDate.AddDays(3);
            documentSubmission4.CreatedAt = currentDate.AddDays(4);

            DatabaseContext.Entry(documentSubmission1).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.Entry(documentSubmission2).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);
            DatabaseContext.Entry(documentSubmission3).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission3);
            DatabaseContext.Entry(documentSubmission4).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission4);

            DatabaseContext.SaveChanges();

            var expected = new List<DocumentSubmission>() { documentSubmission4, documentSubmission3 };

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(queryGuid, team, SubmissionState.Approved, pageSize, page);

            result.Total.Should().Be(3);
            result.DocumentSubmissions.Should().Equal(expected);
        }

        [Test]
        public void GetDocumentSubmissionsByResidentIdDoesNotReturnHiddenDocuments()
        {
            var queryGuid = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(queryGuid);
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "testTeam";
            var page = 1;
            var pageSize = 2;
            var currentDate = new DateTime();

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission1.Team = team;
            documentSubmission1.IsHidden = true;
            var documentSubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission2.Team = team;
            var documentSubmission3 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission3.Team = team;
            var documentSubmission4 = TestDataHelper.DocumentSubmissionWithResidentId(queryGuid, evidenceRequest);
            documentSubmission4.IsHidden = true;
            documentSubmission4.Team = team;

            documentSubmission1.CreatedAt = currentDate.AddDays(1);
            documentSubmission2.CreatedAt = currentDate.AddDays(2);
            documentSubmission3.CreatedAt = currentDate.AddDays(3);
            documentSubmission4.CreatedAt = currentDate.AddDays(4);

            DatabaseContext.Entry(documentSubmission1).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.Entry(documentSubmission2).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);
            DatabaseContext.Entry(documentSubmission3).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission3);
            DatabaseContext.Entry(documentSubmission4).State = EntityState.Modified;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission4);

            DatabaseContext.SaveChanges();

            var expected = new List<DocumentSubmission>() { documentSubmission3, documentSubmission2 };

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(queryGuid, team, null, pageSize, page);

            result.Total.Should().Be(2);
            result.DocumentSubmissions.Should().Equal(expected);
        }

        [Test]
        public void GetClaimIdsForResidentsWithGroupIdsReturnsFilledObject()
        {
            var residentId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(residentId);
            var currentDate = new DateTime();

            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(residentId, evidenceRequest);
            var documentSubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(residentId, evidenceRequest);

            documentSubmission1.CreatedAt = currentDate.AddDays(1);
            documentSubmission2.CreatedAt = currentDate.AddDays(2);

            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);

            DatabaseContext.SaveChanges();

            var testBackfillObject = new GroupResidentIdClaimIdBackfillObject()
            {
                ResidentId = residentId,
                GroupId = groupId
            };

            var resultObject =
                _classUnderTest.GetClaimIdsForResidentsWithGroupIds(
                    new List<GroupResidentIdClaimIdBackfillObject>() { testBackfillObject });

            testBackfillObject.ClaimIds = new List<string>()
            {
                documentSubmission2.ClaimId, documentSubmission1.ClaimId
            };

            resultObject.Should().Equal(testBackfillObject);

        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithResidentIdAndState(EvidenceRequestsSearchQuery request)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();

            var resident1 = TestDataHelper.Resident();
            var resident2 = TestDataHelper.Resident();

            evidenceRequest1.Team = "some-service";
            evidenceRequest2.Team = "some-other-service";
            evidenceRequest3.Team = request.Team;

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

            evidenceRequest1.Team = "some-other-service";
            evidenceRequest2.Team = request.Team;

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

            evidenceRequest1.Team = "some-other-service";
            evidenceRequest2.Team = request.Team;

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

            evidenceRequest1.Team = "some-other-service";
            evidenceRequest2.Team = "development-team-staging";

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

            evidenceRequest1.Team = "some-other-service";
            evidenceRequest2.Team = "development-team-staging";

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);

            DatabaseContext.SaveChanges();
        }

        public List<EvidenceRequest> ExpectedEvidenceRequestsWithDocumentSubmissions(EvidenceRequestsSearchQuery request)
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();


            var documentsubmission1 = TestDataHelper.DocumentSubmission();
            var documentsubmission2 = TestDataHelper.DocumentSubmission();
            var documentsubmission3 = TestDataHelper.DocumentSubmission();
            var documentsubmission4 = TestDataHelper.DocumentSubmission();
            var documentsubmission5 = TestDataHelper.DocumentSubmission();
            var documentsubmission6 = TestDataHelper.DocumentSubmission();

            var resident = TestDataHelper.Resident();
            resident.Id = (Guid) request.ResidentId;

            evidenceRequest1.Team = request.Team;
            evidenceRequest2.Team = request.Team;
            evidenceRequest3.Team = request.Team;

            evidenceRequest1.ResidentId = resident.Id;
            evidenceRequest2.ResidentId = resident.Id;
            evidenceRequest3.ResidentId = resident.Id;

            evidenceRequest1.CreatedAt = DateTime.Today.AddDays(3);
            evidenceRequest2.CreatedAt = DateTime.Today.AddDays(1);
            evidenceRequest3.CreatedAt = DateTime.Today;

            documentsubmission1.CreatedAt = DateTime.Today.AddDays(3);
            documentsubmission2.CreatedAt = DateTime.Today.AddDays(1);
            documentsubmission3.CreatedAt = DateTime.Today;

            evidenceRequest1.DocumentSubmissions.Add(documentsubmission1);
            evidenceRequest1.DocumentSubmissions.Add(documentsubmission2);
            evidenceRequest2.DocumentSubmissions.Add(documentsubmission3);
            evidenceRequest2.DocumentSubmissions.Add(documentsubmission4);
            evidenceRequest3.DocumentSubmissions.Add(documentsubmission5);
            evidenceRequest3.DocumentSubmissions.Add(documentsubmission6);

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest3);

            DatabaseContext.DocumentSubmissions.Add(documentsubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission2);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission3);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission4);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission5);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission6);

            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest3);
            expected.Add(evidenceRequest2);
            expected.Add(evidenceRequest1);

            return expected;
        }
        [Test]
        public void UpdateResidentIdForDocumentSubmissionUpdatesDocumentSubmissionsCorrectly()
        {
            var newResident = TestDataHelper.Resident();
            var originalResident = TestDataHelper.Resident();
            var team = "Fake team";
            DatabaseContext.Residents.Add(newResident);
            DatabaseContext.Residents.Add(originalResident);
            DatabaseContext.SaveChanges();

            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var documentsubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(originalResident.Id, evidenceRequest);
            documentsubmission1.Team = team;
            var documentsubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(originalResident.Id, evidenceRequest);
            documentsubmission2.Team = team;
            var documentsubmission3 = TestDataHelper.DocumentSubmissionWithResidentId(originalResident.Id, evidenceRequest);
            documentsubmission3.Team = team;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission2);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission3);

            DatabaseContext.SaveChanges();

            _classUnderTest.UpdateResidentIdForDocumentSubmission(newResident.Id, new[] { originalResident.Id });

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(newResident.Id, team);
            result.Total.Should().Be(3);

        }
        [Test]
        public void UpdateVisibilityForDocumentSubmissionUpdatesDocumentSubmissionsCorrectly()
        {
            var resident = TestDataHelper.Resident();
            var team = "Fake team";
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var documentsubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(resident.Id, evidenceRequest);
            documentsubmission1.Team = team;
            var documentsubmission2 = TestDataHelper.DocumentSubmissionWithResidentId(resident.Id, evidenceRequest);
            documentsubmission2.Team = team;
            var documentsubmission3 = TestDataHelper.DocumentSubmissionWithResidentId(resident.Id, evidenceRequest);
            documentsubmission3.Team = team;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission2);
            DatabaseContext.DocumentSubmissions.Add(documentsubmission3);

            DatabaseContext.SaveChanges();

            _classUnderTest.UpdateVisibilityDocumentSubmission(documentsubmission2.Id, true);

            var result = _classUnderTest.GetPaginatedDocumentSubmissionsByResidentId(resident.Id, team);
            result.Total.Should().Be(2);
            result.DocumentSubmissions.Should().NotContain(documentsubmission2);
            result.DocumentSubmissions[0].IsHidden.Should().BeFalse();
            result.DocumentSubmissions[1].IsHidden.Should().BeFalse();
        }

    }
}
