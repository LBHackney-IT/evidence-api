using System;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
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
        private AppOptions _options;

        [SetUp]
        public void SetUp()
        {
            _options = _fixture.Create<AppOptions>();
            var client = _messageHandler.CreateClient();
            _classUnderTest = new DocumentsApiGateway(client, _options);
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

            _messageHandler.SetupRequest(HttpMethod.Post, $"{_options.DocumentsApiUrl}api/v1/claims", async request =>
            {
                var json = await request.Content.ReadAsStringAsync().ConfigureAwait(true);
                var body = JsonConvert.DeserializeObject<ClaimRequest>(json);
                return body.ServiceAreaCreatedBy == claimRequest.ServiceAreaCreatedBy &&
                    body.UserCreatedBy == claimRequest.UserCreatedBy &&
                    body.ApiCreatedBy == claimRequest.ApiCreatedBy &&
                    body.RetentionExpiresAt == claimRequest.RetentionExpiresAt &&
                    request.Headers.Authorization.ToString() == _options.DocumentsApiPostClaimsToken;
            })
                .ReturnsResponse(_claimResponseFixture, "application/json");

            var result = await _classUnderTest.GetClaim(claimRequest).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedClaim);
        }

        [Test]
        public async Task CanCreateUploadPolicyWithValidParameters()
        {
            Guid id = Guid.NewGuid();

            var expectedS3UploadPolicy = JsonConvert.DeserializeObject<S3UploadPolicy>(_s3UploadPolicyResponse);

            _messageHandler.SetupRequest(HttpMethod.Post, $"{_options.DocumentsApiUrl}api/v1/documents/{id}/upload_policies", async request =>
            {
                var json = await request.Content.ReadAsStringAsync().ConfigureAwait(true);
                var body = JsonConvert.DeserializeObject<Guid>(json);
                return body == id &&
                    request.Headers.Authorization.ToString() == _options.DocumentsApiPostDocumentsToken;
            })
                .ReturnsResponse(_s3UploadPolicyResponse, "application/json");

            var result = await _classUnderTest.CreateUploadPolicy(id).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedS3UploadPolicy);
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

        private string _s3UploadPolicyResponse = @"{
            ""url"": ""string"",
            ""fields"": {
                ""acl"": ""private"",
                ""key"": ""uuid-document-id"",
                ""X-Amz-Server-Side-Encryption"": ""AES256"",
                ""bucket"": ""documents-api-dev-documents"",
                ""X-Amz-Algorithm"": ""AWS4-HMAC-SHA256"",
                ""X-Amz-Credential"": ""SECRET/20210113/eu-west-2/s3/aws4_request"",
                ""X-Amz-Date"": ""20210113T154042Z"",
                ""Policy"": ""base64 encoded policy"",
                ""X-Amz-Signature"": ""aws generated signature""
            }
        }";
    }
}
