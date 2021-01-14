using System;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;
using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;
using Moq;
using System.Net.Http;
using Newtonsoft.Json;
using Moq.Contrib.HttpClient;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DocumentsApiGatewayTests
    {
        private readonly IFixture _fixture = new Fixture();
        private Mock<HttpMessageHandler> _messageHandler = new Mock<HttpMessageHandler>();
        private DocumentsApiGateway _classUnderTest;
        private string _baseAddress = "https://foo.test.com";

        [SetUp]
        public void SetUp()
        {
            var client = _messageHandler.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            _classUnderTest = new DocumentsApiGateway(client);
        }

        [Test]
        public async Task CanGetAClaimWithValidParameters()
        {
            var claimRequest = _fixture.Build<ClaimRequest>()
                .With(x => x.ServiceAreaCreatedBy, "service-area")
                .With(x => x.UserCreatedBy, "user@email")
                .With(x => x.ApiCreatedBy, "evidence-api")
                .Create();
            var document = _fixture.Create<Document>();

            var expectedClaim = JsonConvert.DeserializeObject<Claim>(_claimResponseFixture);

            _messageHandler.SetupRequest(HttpMethod.Post, $"{_baseAddress}/api/v1/claims", async request =>
            {
                var json = await request.Content.ReadAsStringAsync().ConfigureAwait(true);
                var body = JsonConvert.DeserializeObject<ClaimRequest>(json);
                return body.ServiceAreaCreatedBy == claimRequest.ServiceAreaCreatedBy &&
                    body.UserCreatedBy == claimRequest.UserCreatedBy &&
                    body.ApiCreatedBy == claimRequest.ApiCreatedBy &&
                    body.RetentionExpiresAt == claimRequest.RetentionExpiresAt;
            })
                .ReturnsResponse(_claimResponseFixture, "application/json");

            var result = await _classUnderTest.GetClaim(claimRequest).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedClaim);
        }

        private string _claimResponseFixture = @"{
            ""serviceCreatedBy"": ""711"",
            ""apiCreatedBy"": ""evidence-api"",
            ""userCreatedBy"": ""name.surname@hackney.gov.uk"",
            ""retentionExpiresAt"": ""2021-01-14T14:32:15.377Z"",
            ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""createdAt"": ""2021-01-14T14:32:15.377Z"",
            ""document"": {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""createdAt"": ""2021-01-14T14:32:15.377Z"",
                ""fileSize"": 25300,
                ""fileType"": ""image/png""
            }
        }";
    }
}
