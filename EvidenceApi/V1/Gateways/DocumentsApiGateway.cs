using System;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.Boundary.Request;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways
{
    public class DocumentsApiGateway : IDocumentsApiGateway
    {
        public async Task<Claim> GetClaim(ClaimRequest request)
        {

            var uri = new Uri($"{AppOptions.DocumentsApiUrl}/api/v1/claims", UriKind.Relative);
            HttpClient client = new HttpClient();
            var body = JsonConvert.SerializeObject(request);
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, jsonString).ConfigureAwait(true);
            var claimJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var claim = JsonConvert.DeserializeObject<Claim>(claimJsonString);
            return claim;
        }
    }
}