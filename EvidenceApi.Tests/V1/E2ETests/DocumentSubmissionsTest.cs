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
        private PaginatedClaimResponse _paginatedClaimResponse;
        private Document _document;
        private S3UploadPolicy _createdUploadPolicy;
        private readonly Guid _id = Guid.NewGuid();
        private readonly Guid _groupId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.Clear();

            _document = _fixture.Build<Document>()
                .With(x => x.Id, _id)
                .Create();
            _createdClaim = _fixture.Build<Claim>()
                .With(x => x.Document, _document)
                .Create();

            _createdClaim.GroupId = _groupId;

            _createdUploadPolicy = _fixture.Create<S3UploadPolicy>();

            _paginatedClaimResponse = new PaginatedClaimResponse() { Claims = new List<Claim>() { _createdClaim } };

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/claims/{_createdClaim.Id}").UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );

            DocumentsApiServer.Given(Request.Create().WithParam("groupId", _groupId.ToString()).WithParam("limit", "5000").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200).WithBody(JsonConvert.SerializeObject(_paginatedClaimResponse)));

            DocumentsApiServer.Given(
                Request.Create().WithPath("/api/v1/claims")
            ).RespondWith(
                Response.Create().WithStatusCode(201).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/documents/{_id}/upload_policies")
            ).RespondWith(
                Response.Create().WithStatusCode(200).WithBody(
                    JsonConvert.SerializeObject(_createdUploadPolicy)
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

        [TestCase(false)]
        [TestCase(true)]
        public async Task CanUpdateDocumentSubmissionVisibilityWithValidParameters(bool requiredVisibility)
        {
            // Arrange
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            var documentSubmission = TestDataHelper.DocumentSubmission();
            documentSubmission.EvidenceRequest = evidenceRequest;
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();
            DatabaseContext.Entry(documentSubmission).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            var createdDocumentSubmission = DatabaseContext.DocumentSubmissions.First();
            var uri = new Uri($"api/v1/document_submissions/{createdDocumentSubmission.Id}/visibility", UriKind.Relative);
            string body = $@"
            {{
                ""documentHidden"": {requiredVisibility.ToString().ToLower()}
            }}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PatchAsync(uri, jsonString).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponse>(json);
            result.IsHidden.Should().Be(requiredVisibility);
        }


        [Ignore("Potential race conditions causing intermittent failure - need to be looked at")]
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

            var expected = documentSubmission.ToResponse(null, (Guid) documentSubmission.EvidenceRequestId, null, null, _createdClaim);

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

        [Ignore("Potential race conditions causing intermittent failure - need to be looked at")]
        [Test]
        public async Task CanFindDocumentSubmissionsWithValidParameters()
        {
            var documentType = TestDataHelper.DocumentType("passport-scan");

            var resident = TestDataHelper.ResidentWithId(Guid.NewGuid());

            var team = "Development Housing Team";

            var residentTeamGroupId = new ResidentsTeamGroupId() { Resident = resident, GroupId = _groupId, Team = team };

            var evidenceRequestId = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Id = evidenceRequestId;
            evidenceRequest.Team = team;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(resident.Id, evidenceRequest);
            documentSubmission1.DocumentTypeId = documentType.Id;
            documentSubmission1.Team = team;
            documentSubmission1.ClaimId = _createdClaim.Id.ToString();

            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/document_submissions?team=Development+Housing+Team&residentId={documentSubmission1.ResidentId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponseObject>(json);

            var expectedDocType = new DocumentType()
            {
                Description =
                    "A valid passport open at the photo page",
                Enabled =
                    true,
                Id =
                    "passport-scan",
                Title =
                    "Passport",
            };

            var expected = new DocumentSubmissionResponseObject
            {
                DocumentSubmissions = new List<DocumentSubmissionResponse>()
                {
                    documentSubmission1.ToResponse(expectedDocType, documentSubmission1.EvidenceRequestId, null, null, _createdClaim),
                },
                Total = 1
            };

            response.StatusCode.Should().Be(200);
            result.Should().BeEquivalentTo(expected);
        }

        [Ignore("Potential race conditions causing intermittent failure - need to be looked at")]
        [Test]
        public async Task ReturnsDocumentSubmissionsWithValidParametersAndState()
        {
            var residentId = Guid.NewGuid();
            var resident = TestDataHelper.ResidentWithId(residentId);
            var currentDate = new DateTime();
            var team = "Development Housing Team";

            var residentTeamGroupId = new ResidentsTeamGroupId() { GroupId = _groupId, Resident = resident, Team = team };

            var evidenceRequestId = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Id = evidenceRequestId;
            evidenceRequest.Team = team;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);

            DatabaseContext.SaveChanges();

            var documentSubmission1 = TestDataHelper.DocumentSubmissionWithResidentId(residentId, evidenceRequest);
            documentSubmission1.State = SubmissionState.Approved;

            documentSubmission1.CreatedAt = currentDate.AddDays(1);
            documentSubmission1.ClaimId = _createdClaim.Id.ToString();

            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);

            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/document_submissions?team=Development+Housing+Team&residentId={residentId}&state=Approved", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponseObject>(json);

            var expected = new DocumentSubmissionResponseObject()
            {
                DocumentSubmissions = new List<DocumentSubmissionResponse>()
                {
                    documentSubmission1.ToResponse(null, documentSubmission1.EvidenceRequestId, null, null, _createdClaim),

                    },
                Total = 1
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

        [Test]
        public async Task CanCreateDocumentSubmissionWithoutEvidenceRequestWithValidParams()
        {
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          $"\"residentId\": \"{resident.Id}\"," +
                          "\"team\": \"Development Housing Team\"," +
                          "\"userCreatedBy\": \"test-user\"," +
                          "\"staffSelectedDocumentTypeId\": \"passport-scan\"," +
                          "\"documentDescription\": \"some document description\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync();

            var created = DatabaseContext.DocumentSubmissions.First();

            var formattedCreatedAt = JsonConvert.SerializeObject(created.CreatedAt);
            var formattedValidUntil = JsonConvert.SerializeObject(_createdClaim.ValidUntil);
            var formattedRetentionExpiresAt = JsonConvert.SerializeObject(_createdClaim.RetentionExpiresAt);
            string expected = "{" +
                               $"\"id\":\"{created.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}," +
                               $"\"claimId\":\"{_createdClaim.Id}\"," +
                               $"\"team\":\"{created.Team}\"," +
                               $"\"residentId\":\"{created.ResidentId}\"," +
                               $"\"claimValidUntil\":{formattedValidUntil}," +
                               $"\"retentionExpiresAt\":{formattedRetentionExpiresAt}," +
                               $"\"state\":\"APPROVED\"," +
                               "\"staffSelectedDocumentType\":{\"id\":\"passport-scan\",\"title\":\"Passport Scan\",\"description\":\"A valid passport open at the photo page\",\"enabled\":true}," +
                               $"\"uploadPolicy\":{JsonConvert.SerializeObject(_createdUploadPolicy, Formatting.None)}," +
                               $"\"document\":" + "{" + $"\"id\":\"{_document.Id}\",\"fileSize\":{_document.FileSize},\"fileType\":\"{_document.FileType}\",\"description\":\"{_document.Description}\"" + "}" +
                               "}";

            json.Should().Be(expected);
        }

        [Test]
        public async Task CannotCreateDocumentSubmissionWithoutEvidenceRequestWithInvalidParameters()
        {
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          $"\"residentId\": \"{resident.Id}\"," +
                          "\"team\": \"\"," +
                          "\"userCreatedBy\": \"test-user\"," +
                          "\"staffSelectedDocumentTypeId\": \"passport-scan\"," +
                          "\"documentDescription\": \"some document description\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be($"\"'Team' must not be empty.\"");
        }

        [Test]
        public async Task CannotCreateDocumentSubmissionWithoutEvidenceRequestIfResidentDoesNotExist()
        {
            var residentId = Guid.NewGuid();
            string body = "{" +
                          $"\"residentId\": \"{residentId}\"," +
                          "\"team\": \"Development Housing Team\"," +
                          "\"userCreatedBy\": \"test-user\"," +
                          "\"staffSelectedDocumentTypeId\": \"passport-scan\"," +
                          "\"documentDescription\": \"some document description\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be($"\"A resident with ID {residentId} does not exist.\"");
        }
    }
}
