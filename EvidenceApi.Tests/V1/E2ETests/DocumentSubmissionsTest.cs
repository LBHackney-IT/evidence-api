using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using EvidenceApi.V1.Infrastructure;
using AutoFixture;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class DocumentSubmissionsTest : IntegrationTests<Startup>
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public async Task CanCreateDocumentSubmissionWithValidParams()
        {
            var entity = _fixture.Build<EvidenceRequestEntity>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<string> { "Email" })
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
                               $"\"claimId\":\"{created.ClaimId}\"," +
                               $"\"rejectionReason\":null," +
                               $"\"state\":\"PENDING\"," +
                               "\"documentType\":\"passport-scan\"}";
            json.Should().Be(expected);
        }

        [Test]
        public async Task UnsuccessfulWithInvalidParams()
        {
            var entity = _fixture.Build<EvidenceRequestEntity>()
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<string> { "Email" })
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
    }
}
