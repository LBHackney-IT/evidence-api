using System;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.Boundary.Request;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EvidenceApi.V1.Domain;
using System.Net.Http.Headers;

namespace EvidenceApi.V1.Gateways
{
    public class DocumentsApiGateway : IDocumentsApiGateway
    {
        private readonly HttpClient _client;
        private readonly AppOptions _options;
        public DocumentsApiGateway(HttpClient httpClient, AppOptions options)
        {
            _client = httpClient;
            _options = options;

            _client.BaseAddress = _options.DocumentsApiUrl;

        }
        public async Task<Claim> GetClaim(ClaimRequest request)
        {

            var uri = new Uri("/api/v1/claims", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPostClaimsToken);

            // SerializeBody
            var body = JsonConvert.SerializeObject(request);
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);

            // DeserializeResponse
            var claimJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var claim = JsonConvert.DeserializeObject<Claim>(claimJsonString);

            return claim;
        }

        public async Task<S3UploadPolicy> CreateUploadPolicy(Guid id)
        {
            var uri = new Uri($"/api/v1/documents/${id}/upload_policies", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.DocumentsApiPostDocumentsToken);
            var body = JsonConvert.SerializeObject(id);
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);
            var uploadPolicyJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var s3UploadPolicy = JsonConvert.DeserializeObject<S3UploadPolicy>(uploadPolicyJsonString);
            return s3UploadPolicy;
        }
    }
}
