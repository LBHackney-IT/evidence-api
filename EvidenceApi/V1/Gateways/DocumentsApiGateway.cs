using System;
using System.Net;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.Boundary.Request;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EvidenceApi.V1.Domain;
using System.Net.Http.Headers;
using EvidenceApi.V1.Boundary.Response.Exceptions;

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

            var jsonString = SerializeClaimRequest(request);

            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new DocumentsApiException($"Incorrect status code returned: {response.StatusCode}");
            }

            return await DeserializeResponse<Claim>(response).ConfigureAwait(true);
        }

        public async Task<Claim> UpdateClaim(Guid id, ClaimUpdateRequest request)
        {
            var uri = new Uri($"api/v1/claims/{id}", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPatchClaimsToken);

            var jsonString = SerializeClaimUpdateRequest(request);

            var response = await _client.PatchAsync(uri, jsonString).ConfigureAwait(true);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorBody = await DeserializeResponse<string>(response).ConfigureAwait(true);
                throw new DocumentsApiException(errorBody);
            }

            return await DeserializeResponse<Claim>(response).ConfigureAwait(true);
        }

        public async Task UploadDocument(Guid documentId, DocumentSubmissionRequest request)
        {
            var uri = new Uri($"api/v1/documents/{documentId}", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPostDocumentsToken);
            var documentUploadRequest = new DocumentUploadRequest();
            documentUploadRequest.Base64Document = request.Base64Document;
            var jsonString = SerializeDocumentUploadRequest(documentUploadRequest);
            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new DocumentsApiException($"Incorrect status code returned: {response.StatusCode}");
            }
        }

        public async Task<Claim> GetClaimById(string id)
        {
            var uri = new Uri($"api/v1/claims/{id}", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiGetClaimsToken);
            var response = await _client.GetAsync(uri).ConfigureAwait(true);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new DocumentsApiException($"Incorrect status code returned: {response.StatusCode}");
            }
            return await DeserializeResponse<Claim>(response).ConfigureAwait(true);
        }

        private static StringContent SerializeDocumentUploadRequest(DocumentUploadRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        private static StringContent SerializeClaimRequest(ClaimRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        private static StringContent SerializeClaimUpdateRequest(ClaimUpdateRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
