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
        public async Task<Claim> CreateClaim(ClaimRequest request)
        {

            var uri = new Uri("api/v1/claims", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPostClaimsToken);

            var jsonString = SerializeBody(request);

            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);

            return await DeserializeResponse<Claim>(response).ConfigureAwait(true);
        }

        public async Task<S3UploadPolicy> CreateUploadPolicy(Guid id)
        {
            var uri = new Uri($"api/v1/documents/{id}/upload_policies", UriKind.Relative);
            Console.WriteLine("==TOKEN== {0}", _options.DocumentsApiPostDocumentsToken);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPostDocumentsToken);
            var response = await _client.PostAsync(uri, null).ConfigureAwait(true);
            Console.WriteLine("==POLICY==");
            return await DeserializeResponse<S3UploadPolicy>(response).ConfigureAwait(true);
        }

        private static StringContent SerializeBody(ClaimRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            Console.WriteLine(jsonString);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
