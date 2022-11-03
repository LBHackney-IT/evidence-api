using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class DocumentSubmissionsTest : IntegrationTests<Startup>
    {
        private readonly IFixture _fixture = new Fixture();
        private Claim _createdClaim;
        private Document _document;
        private readonly Guid _id = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _document = _fixture.Build<Document>()
                .With(x => x.Id, _id)
                .Create();
            _createdClaim = _fixture.Build<Claim>()
                .With(x => x.Document, _document)
                .Create();

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/claims/{_createdClaim.Id}").UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );
        }

        [Test]
        public async Task CanUpdateDocumentSubmissionStateWithValidParameters()
        {
            // Arrange
            var teamName = "Development Housing Team";
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Team = teamName;

            evidenceRequest.DocumentTypes = new List<string> { "passport-scan" };
            evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email };

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);

            var documentSubmission = TestDataHelper.DocumentSubmission();
            documentSubmission.EvidenceRequest = evidenceRequest;
            documentSubmission.DocumentTypeId = "passport-scan";

            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            DatabaseContext.Entry(documentSubmission).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var createdDocumentSubmission = DatabaseContext.DocumentSubmissions.First();

            var uri = new Uri($"api/v1/document_submissions/{createdDocumentSubmission.Id}", UriKind.Relative);
            string body = @"
            {
                ""state"": ""UPLOADED"",
                ""staffSelectedDocumentTypeId"": ""drivers-licence""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PatchAsync(uri, jsonString).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponse>(json);

            var documentType = TestDataHelper.DocumentType("passport-scan");
            var staffSelectedDocumentType = TestDataHelper.GetStaffSelectedDocumentTypeByTeamName("drivers-licence", teamName);
            var expected = createdDocumentSubmission.ToResponse(documentType, createdDocumentSubmission.EvidenceRequestId, staffSelectedDocumentType);
            result.Should().BeEquivalentTo(expected);
        }

        [Ignore("need to decide how to proceed with fk constraint")]
        [Test]
        public async Task CanUpdateDocumentSubmissionStateOnlyWhenRejected()
        {
            // Arrange
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Team = "Development Housing Team";

            evidenceRequest.DocumentTypes = new List<string> { "passport-scan" };
            evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email };
            evidenceRequest.ResidentId = resident.Id;

            DatabaseContext.Residents.Add(resident);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);

            var documentSubmission = TestDataHelper.DocumentSubmission();
            documentSubmission.EvidenceRequest = evidenceRequest;
            documentSubmission.DocumentTypeId = "passport-scan";

            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            DatabaseContext.Entry(documentSubmission).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var createdDocumentSubmission = DatabaseContext.DocumentSubmissions.First();

            var uri = new Uri($"api/v1/document_submissions/{createdDocumentSubmission.Id}", UriKind.Relative);
            string body = @"
            {
                ""state"": ""REJECTED"",
                ""userUpdatedBy"": ""TestEmail@hackney.gov.uk"",
                ""rejectionReason"": ""This is the rejection reason""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PatchAsync(uri, jsonString).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponse>(json);

            result.State.Should().Be("REJECTED");
        }

        [Test]
        public async Task CanFindDocumentSubmissionWithValidId()
        {
            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            documentSubmission.ClaimId = _createdClaim.Id.ToString();
            documentSubmission.DocumentTypeId = "passport-scan";
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"api/v1/document_submissions/{documentSubmission.Id}", UriKind.Relative);

            var responseFind = await Client.GetAsync(uri).ConfigureAwait(true);
            var jsonFind = await responseFind.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponse>(jsonFind);

            var expected = documentSubmission.ToResponse(null, documentSubmission.EvidenceRequestId, null, null, _createdClaim);

            responseFind.StatusCode.Should().Be(200);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReturnNotFoundWhenCannotFindDocumentSubmission()
        {
            var fakeId = Guid.NewGuid();
            var uri = new Uri($"api/v1/document_submissions/{fakeId}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task ReturnBadRequestWhenIssueDuringFindDocumentSubmission()
        {
            // Arrange
            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            documentSubmission.ClaimId = _createdClaim.Id.ToString();
            documentSubmission.DocumentTypeId = "passport-scan";
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            DocumentsApiServer.Reset();
            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/document_submissions/{documentSubmission.Id}")
            ).RespondWith(
                Response.Create().WithStatusCode(404)
            );

            var uri = new Uri($"api/v1/document_submissions/{documentSubmission.Id}", UriKind.Relative);

            // Act
            var result = await Client.GetAsync(uri).ConfigureAwait(true);

            // Assert
            result.StatusCode.Should().Be(400);
        }

        [Ignore("need to decide how to proceed with fk constraint")]
        [Test]
        public async Task CanFindDocumentSubmissionsWithValidParameters()
        {
            var documentType = TestDataHelper.DocumentType("passport-scan");

            var evidenceRequestId = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Id = evidenceRequestId;
            evidenceRequest.Team = "Development Housing Team";

            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.EvidenceRequestId = evidenceRequest.Id;
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.ClaimId = _createdClaim.Id.ToString();

            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.EvidenceRequestId = evidenceRequest.Id;
            documentSubmission2.DocumentTypeId = "passport-scan";
            documentSubmission2.ClaimId = _createdClaim.Id.ToString();

            //DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/document_submissions?team=Development+Housing+Team&residentId={documentSubmission1.ResidentId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<List<DocumentSubmissionResponse>>(json);

            var expected = new List<DocumentSubmissionResponse>()
            {
                documentSubmission1.ToResponse(documentType, documentSubmission1.EvidenceRequestId, null, null, _createdClaim),
                //documentSubmission2.ToResponse(documentType, documentSubmission2.EvidenceRequestId, null, null, _createdClaim)
            };

            response.StatusCode.Should().Be(200);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReturnBadRequestWhenSearchQueryIsInvalid()
        {
            var team = "";
            var fakeResidentId = Guid.NewGuid();
            var uri = new Uri($"api/v1/document_submissions?team={team}&residentId={fakeResidentId}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }
    }
}
