using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.Boundary.Request;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EvidenceApi.V1.Domain;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        public async Task<string> BackfillClaimsWithGroupIds(List<GroupResidentIdClaimIdBackfillObject> backfillObjects)
        {
            foreach (var backfillObject in backfillObjects)
            {
                //create body
               var jsonString = SerializeBackfillRequest(backfillObject);

               //foreach claim id, call the endpoint and update with the groupid
                foreach (var claimId in backfillObject.ClaimIds)
                {
                    var uri = new Uri($"api/v1/claims/{claimId}", UriKind.Relative);
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiPatchClaimsToken);

                    var response = await _client.PatchAsync(uri, jsonString).ConfigureAwait(true);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var errorBody = await DeserializeResponse<string>(response).ConfigureAwait(true);
                        throw new DocumentsApiException(errorBody);
                    }
                }
            }
            return "Backfill completed successfully";
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

        public async Task<List<Claim>> GetClaimsByIdsThrottled(List<string> claimIds)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiGetClaimsToken);

            var throttler = new SemaphoreSlim(5);
            var tasks = claimIds.Select(async claimId =>
            {
                await throttler.WaitAsync();

                var uri = new Uri($"api/v1/claims/{claimId}", UriKind.Relative);
                var response = await _client.GetAsync(uri);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DocumentsApiException($"Incorrect status code returned: {response.StatusCode}");
                }

                throttler.Release();

                return await DeserializeResponse<Claim>(response);
            });

            var claims = await Task.WhenAll(tasks);
            return claims.ToList();
        }

        public async Task<S3UploadPolicy> CreateUploadPolicy(Guid id)
        {
            var uri = new Uri($"api/v1/documents/{id}/upload_policies", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_options.DocumentsApiGetDocumentsToken);
            var response = await _client.GetAsync(uri).ConfigureAwait(true);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new DocumentsApiException($"Incorrect status code returned: {response.StatusCode}");
            }
            return await DeserializeResponse<S3UploadPolicy>(response).ConfigureAwait(true);
        }

        private static StringContent SerializeBackfillRequest(GroupResidentIdClaimIdBackfillObject request)
        {
            var updateRequest = new ClaimUpdateRequest() { GroupId = request.GroupId };
            var body = JsonConvert.SerializeObject(updateRequest);
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
