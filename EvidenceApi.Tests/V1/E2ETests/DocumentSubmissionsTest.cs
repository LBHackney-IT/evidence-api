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
        private Guid id = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _document = _fixture.Build<Document>()
                .With(x => x.Id, id)
                .Create();
            _createdClaim = _fixture.Build<Claim>()
                .With(x => x.Document, _document)
                .Create();

            DocumentsApiServer.Given(
                Request.Create().WithPath("/api/v1/claims")
            ).RespondWith(
                Response.Create().WithStatusCode(201).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/claims/{_createdClaim.Id}").UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/documents/{id}")
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );
        }


        [Test]
        public async Task CanCreateDocumentSubmissionWithValidParams()
        {
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "proof-of-id" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .With(x => x.Team, "Development Housing Team")
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          "\"base64Document\": \"abcd\"," +
                          "\"documentType\": \"proof-of-id\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            var created = DatabaseContext.DocumentSubmissions.First();

            var formattedCreatedAt = JsonConvert.SerializeObject(created.CreatedAt);
            var formattedValidUntil = JsonConvert.SerializeObject(_createdClaim.ValidUntil);
            var formattedRetentionExpiresAt = JsonConvert.SerializeObject(_createdClaim.RetentionExpiresAt);
            string expected = "{" +
                               $"\"id\":\"{created.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}," +
                               $"\"claimId\":\"{_createdClaim.Id}\"," +
                               $"\"acceptedAt\":null," +
                               $"\"rejectionReason\":null," +
                               $"\"rejectedAt\":null,\"userUpdatedBy\":null," +
                               $"\"claimValidUntil\":{formattedValidUntil}," +
                               $"\"retentionExpiresAt\":{formattedRetentionExpiresAt}," +
                               $"\"state\":\"UPLOADED\"," +
                               "\"documentType\":{\"id\":\"proof-of-id\",\"title\":\"Proof of ID\",\"description\":\"A valid document that can be used to prove identity\"}," +
                               "\"staffSelectedDocumentType\":null," +
                               $"\"document\":" + "{" + $"\"id\":\"{_document.Id}\",\"fileSize\":{_document.FileSize},\"fileType\":\"{_document.FileType}\"" + "}" +
                               "}";

            json.Should().Be(expected);
        }

        [Test]
        public async Task CreateDocumentSubmissionUnsuccessfulWhenNoDocumentTypeProvided()
        {
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          "\"base64Document\": \"abcd\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateDocumentSubmissionUnsuccessfulWhenNoDocumentProvided()
        {
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          "\"documentType\": \"proof-of-id\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CreateDocumentSubmissionUnsuccessfulWhenCannotCreateClaim()
        {
            // Arrange
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            DocumentsApiServer.Reset();
            DocumentsApiServer.Given(
                Request.Create().WithPath("/api/v1/claims")
            ).RespondWith(
                Response.Create().WithStatusCode(404)
            );

            string body = "{" +
                          "\"base64Document\": \"abcd\"," +
                          "\"documentType\": \"proof-of-id\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);

            // Act
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task ReturnNotFoundWhenCannotFindEvidenceRequest()
        {
            var fakeId = "ed0f2bd2-df90-4f01-b7f1-d30e402386d0";
            string body = "{" +
                          "\"base64Document\": \"abcd\"," +
                          "\"documentType\": \"proof-of-id\"" +
                          "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/evidence_requests/{fakeId}/document_submissions", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);
        }


        [Test]
        public async Task CanUpdateDocumentSubmissionStateWithValidParameters()
        {
            // Arrange
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            evidenceRequest.Team = "Development Housing Team";

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
            var staffSelectedDocumentType = TestDataHelper.DocumentType("drivers-licence");
            var expected = createdDocumentSubmission.ToResponse(documentType, staffSelectedDocumentType);
            result.Should().BeEquivalentTo(expected);
        }

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

            var expected = documentSubmission.ToResponse(null, null, _createdClaim);

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

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission1);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission2);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/document_submissions?team=Development+Housing+Team&residentId={evidenceRequest.ResidentId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<List<DocumentSubmissionResponse>>(json);

            var expected = new List<DocumentSubmissionResponse>()
            {
                documentSubmission1.ToResponse(documentType,  null, _createdClaim),
                documentSubmission2.ToResponse(documentType,  null, _createdClaim)
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
