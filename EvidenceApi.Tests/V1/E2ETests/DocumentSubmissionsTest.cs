using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
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
        private S3UploadPolicy _createdUploadPolicy;

        [SetUp]
        public void SetUp()
        {
            Guid id = Guid.NewGuid();
            _document = _fixture.Build<Document>()
                .With(x => x.Id, id)
                .Create();
            _createdClaim = _fixture.Build<Claim>()
                .With(x => x.Document, _document)
                .Create();

            _createdUploadPolicy = _fixture.Create<S3UploadPolicy>();

            DocumentsApiServer.Given(
                Request.Create().WithPath("/api/v1/claims")
            ).RespondWith(
                Response.Create().WithStatusCode(201).WithBody(
                    JsonConvert.SerializeObject(_createdClaim)
                )
            );

            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/documents/{id}/upload_policies")
            ).RespondWith(
                Response.Create().WithStatusCode(201).WithBody(
                    JsonConvert.SerializeObject(_createdUploadPolicy)
                )
            );
        }


        [Test]
        public async Task CanCreateDocumentSubmissionWithValidParams()
        {
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);
            string body = @"
            {
                ""documentType"": ""passport-scan"",
                ""serviceName"": ""service-name"",
                ""requesterEmail"": ""example@email""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            var created = DatabaseContext.DocumentSubmissions.First();

            var formattedCreatedAt = JsonConvert.SerializeObject(created.CreatedAt.ToDateTimeOffset());
            string expected = "{" +
                               $"\"id\":\"{created.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}," +
                               $"\"claimId\":\"{_createdClaim.Id}\"," +
                               $"\"rejectionReason\":null," +
                               $"\"state\":\"PENDING\"," +
                               "\"documentType\":{\"id\":\"passport-scan\",\"title\":\"Passport\",\"description\":\"A valid passport open at the photo page\"}," +
                               $"\"uploadPolicy\":{JsonConvert.SerializeObject(_createdUploadPolicy, Formatting.None)}" +
                               "}";

            json.Should().Be(expected);
        }

        [Test]
        public async Task UnsuccessfulWithInvalidParams()
        {
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}/document_submissions", UriKind.Relative);
            string body = @"
            {
                ""documentType"": """"
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task ReturnNotFoundWhenCannotFindEvidenceRequest()
        {
            var fakeId = "ed0f2bd2-df90-4f01-b7f1-d30e402386d0";
            var uri = new Uri($"api/v1/evidence_requests/{fakeId}/document_submissions", UriKind.Relative);
            string body = @"
            {
                ""documentType"": ""passport-scan"",
                ""serviceName"": ""service-name"",
                ""requesterEmail"": ""example@email""
            }";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task CanUpdateDocumentSubmissionStateWithValidParameters()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();

            evidenceRequest.DocumentTypes = new List<string> { "passport-scan" };
            evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email };

            DatabaseContext.EvidenceRequests.Add(evidenceRequest);

            var documentSubmission = TestDataHelper.DocumentSubmission();
            documentSubmission.EvidenceRequest = evidenceRequest;
            documentSubmission.DocumentTypeId = "passport-scan";

            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();

            DatabaseContext.Entry(documentSubmission).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var createdEvidenceRequest = DatabaseContext.EvidenceRequests.First();
            var createdDocumentSubmission = DatabaseContext.DocumentSubmissions.First();

            var uri = new Uri($"api/v1/document_submissions/{createdDocumentSubmission.Id}", UriKind.Relative);
            string body = @"
            {
                ""state"": ""UPLOADED""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PatchAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonConvert.DeserializeObject<DocumentSubmissionResponse>(json);

            var documentType = TestDataHelper.DocumentType("passport-scan");
            var expected = createdDocumentSubmission.ToResponse(documentType);
            result.Should().BeEquivalentTo(expected);
        }
    }
}
