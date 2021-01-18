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
        public DocumentsApiGateway(HttpClient httpClient)
        {
            _client = httpClient;
        }
        public async Task<Claim> GetClaim(ClaimRequest request)
        {

            var uri = new Uri("/api/v1/claims", UriKind.Relative);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppOptions.DocumentsApiPostClaimsToken);
            var body = JsonConvert.SerializeObject(request);
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(uri, jsonString).ConfigureAwait(true);
            var claimJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var claim = JsonConvert.DeserializeObject<Claim>(claimJsonString);
            return claim;
        }
    }
}
