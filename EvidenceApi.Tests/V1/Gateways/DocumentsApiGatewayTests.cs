using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using Newtonsoft.Json;
using Moq.Contrib.HttpClient;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DocumentsApiGatewayTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<HttpMessageHandler> _messageHandler = new Mock<HttpMessageHandler>();
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
        public async Task CanCreateAClaimWithValidParameters()
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
                .ReturnsResponse(HttpStatusCode.Created, _claimResponseFixture, "application/json");

            var result = await _classUnderTest.CreateClaim(claimRequest).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedClaim);
        }

        [Test]
        public async Task CanCreateUploadPolicyWithValidParameters()
        {
            Guid id = Guid.NewGuid();

            var expectedS3UploadPolicy = JsonConvert.DeserializeObject<S3UploadPolicy>(_s3UploadPolicyResponse);

            _messageHandler.SetupRequest(HttpMethod.Get, $"{_options.DocumentsApiUrl}api/v1/documents/{id}/upload_policies", request =>
                {
                    return request.Headers.Authorization.ToString() == _options.DocumentsApiGetDocumentsToken;
                })
                    .ReturnsResponse(HttpStatusCode.OK, _s3UploadPolicyResponse, "application/json");

            var result = await _classUnderTest.CreateUploadPolicy(id).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedS3UploadPolicy);
        }

        [Test]
        public async Task CanGetAClaim()
        {
            var id = Guid.NewGuid().ToString();
            var expectedClaim = JsonConvert.DeserializeObject<Claim>(_claimResponseFixture);
            _messageHandler.SetupRequest(HttpMethod.Get, $"{_options.DocumentsApiUrl}api/v1/claims/{id}", request =>
                {
                    return request.Headers.Authorization.ToString() == _options.DocumentsApiGetClaimsToken;
                })
                .ReturnsResponse(HttpStatusCode.OK, _claimResponseFixture, "application/json");
            var result = await _classUnderTest.GetClaimById(id).ConfigureAwait(true);

            result.Should().BeEquivalentTo(expectedClaim);
        }

        [Test]
        public async Task CanGetClaimsByIds()
        {
            const int numberOfClaims = 50;
            var claims = Enumerable.Repeat(0, numberOfClaims).Select(_ => _fixture.Create<Claim>()).ToList();

            claims.ForEach(claim => _messageHandler.SetupRequest(HttpMethod.Get,
                    $"{_options.DocumentsApiUrl}api/v1/claims/{claim.Id}",
                    request => request.Headers.Authorization.ToString() == _options.DocumentsApiGetClaimsToken)
                .ReturnsResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(claim), "application/json")
            );

            var claimIds = claims.Select(claim => claim.Id.ToString()).ToList();
            var result = await _classUnderTest.GetClaimsByIdsThrottled(claimIds);

            result.Should().BeEquivalentTo(claims);
        }

        [Test]
        public async Task CanGetClaimsByGroupId()
        {
            var groupId = Guid.NewGuid();
            var claimOne = _fixture.Create<Claim>();
            var claimTwo = _fixture.Create<Claim>();
            var claimThree = _fixture.Create<Claim>();
            claimOne.GroupId = groupId;
            claimTwo.GroupId = groupId;
            claimThree.GroupId = groupId;

            var request = new PaginatedClaimRequest() { GroupId = groupId };

            var claimsList = new List<Claim>() { claimOne, claimTwo, claimThree };

            var paginatedClaimsResponse = new PaginatedClaimResponse() { Claims = claimsList };

            _messageHandler.SetupRequest(HttpMethod.Get,
                    $"{_options.DocumentsApiUrl}api/v1/claims?groupId={groupId}",
                    request => request.Headers.Authorization.ToString() == _options.DocumentsApiGetClaimsToken)
                .ReturnsResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(paginatedClaimsResponse), "application/json");

            var result = await _classUnderTest.GetClaimsByGroupId(request);

            result.Claims.Should().BeEquivalentTo(claimsList);


        }

        [Test]
        public void ShouldThrowWhenGettingClaimsByIdsFails()
        {
            void AddClaimResponse(Guid claimId, HttpStatusCode status, string body)
            {
                _messageHandler.SetupRequest(HttpMethod.Get,
                        $"{_options.DocumentsApiUrl}api/v1/claims/{claimId}",
                        request => request.Headers.Authorization.ToString() == _options.DocumentsApiGetClaimsToken)
                    .ReturnsResponse(status, body, "application/json");
            }

            const int numberOfSuccessfulClaims = 5;
            var successfulClaims = Enumerable.Repeat(0, numberOfSuccessfulClaims)
                .Select(_ => _fixture.Create<Claim>())
                .ToList();
            successfulClaims.ForEach(claim =>
                AddClaimResponse(claim.Id, HttpStatusCode.OK, JsonConvert.SerializeObject(claim)));

            var failedClaimId = Guid.NewGuid();
            const HttpStatusCode failedStatus = HttpStatusCode.InternalServerError;
            AddClaimResponse(failedClaimId, failedStatus, JsonConvert.SerializeObject(new
            {
                status = failedStatus,
                title = "Something went wrong"
            }));

            var claimIds = successfulClaims
                .Select(claim => claim.Id.ToString())
                .Append(failedClaimId.ToString())
                .ToList();

            var ex = Assert.ThrowsAsync<DocumentsApiException>(() => _classUnderTest.GetClaimsByIdsThrottled(claimIds));
            ex.Message.Should().Be($"Incorrect status code returned: {failedStatus}");
        }

        [Test]
        public async Task CanUpdateAClaimWithValidParameters()
        {
            // Arrange
            var id = Guid.NewGuid();
            var claimUpdateRequest = _fixture.Build<ClaimUpdateRequest>()
                .With(x => x.ValidUntil, DateTime.UtcNow)
                .Create();

            var expectedClaim = JsonConvert.DeserializeObject<Claim>(_claimResponseFixture);

            _messageHandler.SetupRequest(HttpMethod.Patch, $"{_options.DocumentsApiUrl}api/v1/claims/{id}", async request =>
                {
                    var json = await request.Content.ReadAsStringAsync().ConfigureAwait(true);
                    var body = JsonConvert.DeserializeObject<ClaimRequest>(json);
                    return body.ValidUntil == claimUpdateRequest.ValidUntil &&
                           request.Headers.Authorization.ToString() == _options.DocumentsApiPatchClaimsToken;
                })
                .ReturnsResponse(HttpStatusCode.OK, _claimResponseFixture, "application/json");

            // Act
            var result = await _classUnderTest.UpdateClaim(id, claimUpdateRequest).ConfigureAwait(true);

            // Assert
            result.Should().BeEquivalentTo(expectedClaim);
        }

        [Test]
        public async Task CanUpdateClaimWithGroupId()
        {
            var claimId = Guid.NewGuid();
            var groupId = new Guid("3da21f64-5717-4562-b3fc-2c963f66afb3");
            var claimUpdateRequest = _fixture.Build<ClaimUpdateRequest>()
                .With(x => x.GroupId, groupId)
                .Create();

            var mockBackfillObjectList = new List<GroupResidentIdClaimIdBackfillObject>()
            {
                new GroupResidentIdClaimIdBackfillObject()
                {
                    ClaimIds = new List<string>()
                    {
                        claimId.ToString()
                    },
                    GroupId = groupId
                }
            };

            var expectedClaim = JsonConvert.DeserializeObject<Claim>(_claimResponseFixture);

            var expectedClaimBackfillResponse = new List<ClaimBackfillResponse>()
            {
                new ClaimBackfillResponse() { GroupId = expectedClaim.GroupId, ClaimId = expectedClaim.Id }
            };

            _messageHandler.SetupRequest(HttpMethod.Patch, $"{_options.DocumentsApiUrl}api/v1/claims/{claimId}", request =>
               {
                   return request.Headers.Authorization.ToString() == _options.DocumentsApiPatchClaimsToken;
               })
                .ReturnsResponse(HttpStatusCode.OK, _claimResponseFixture, "application/json");

            var result = await _classUnderTest.BackfillClaimsWithGroupIds(mockBackfillObjectList);

            result.Should().BeEquivalentTo(expectedClaimBackfillResponse);
        }

        private string _claimResponseFixture = @"{
            ""serviceCreatedBy"": ""711"",
            ""apiCreatedBy"": ""evidence-api"",
            ""userCreatedBy"": ""name.surname@hackney.gov.uk"",
            ""retentionExpiresAt"": ""2021-01-14T14:32:15.377Z"",
            ""validUntil"": ""2021-01-14T14:32:15.377Z"",
            ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""createdAt"": ""2021-01-14T14:32:15.377Z"",
            ""groupId"": ""3da21f64-5717-4562-b3fc-2c963f66afb3"",
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
