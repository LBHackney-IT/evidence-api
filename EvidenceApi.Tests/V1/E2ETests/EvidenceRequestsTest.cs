using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class EvidenceRequestsTest : IntegrationTests<Startup>
    {
        [Test]
        public async Task CanCreateEvidenceRequestWithValidParams()
        {
            var uri = new Uri($"api/v1/evidence_requests", UriKind.Relative);
            string body = @"
            {
                ""resident"": {
                    ""name"": ""Frodo Baggins"",
                    ""email"": ""frodo@bagend.com,"",
                    ""phoneNumber"": ""+447123456789""
                },
                ""deliveryMethods"": [""SMS""],
                ""documentTypes"": [""passport-scan""],
                ""serviceRequestedBy"": ""development-team-staging""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            var created = DatabaseContext.EvidenceRequests.First();
            var resident = DatabaseContext.Residents.First();

            var formattedCreatedAt = JsonConvert.SerializeObject(created.CreatedAt.ToDateTimeOffset());
            string expected = "{"
                              + "\"resident\":{"
                              + $"\"id\":\"{resident.Id}\","
                              + "\"name\":\"Frodo Baggins\","
                              + "\"email\":\"frodo@bagend.com,\","
                              + "\"phoneNumber\":\"+447123456789\""
                              + "},"
                              + "\"deliveryMethods\":[\"SMS\"],"
                              + "\"documentTypes\":[\"passport-scan\"],"
                              + "\"serviceRequestedBy\":\"development-team-staging\","
                              + $"\"id\":\"{created.Id}\","
                              + $"\"createdAt\":{formattedCreatedAt}"
                              + "}";

            json.Should().Be(expected);
        }

        [Test]
        public async Task UnsuccessfulWithInvalidParams()
        {
            var uri = new Uri($"api/v1/evidence_requests", UriKind.Relative);
            string body = @"
            {
                ""resident"": {
                    ""name"": ""Frodo Baggins"",
                    ""email"": ""frodo@bagend.com,"",
                    ""phoneNumber"": ""+447123456789""
                },
                ""deliveryMethods"": [""FOO""],
                ""documentTypes"": [""passport-scan""],
                ""serviceRequestedBy"": ""development-team-staging""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task UnsuccessfulWhenResidentIsInvalid()
        {
            var uri = new Uri($"api/v1/evidence_requests", UriKind.Relative);
            string body = @"
            {
                ""resident"": {
                    ""name"": ""Frodo Baggins"",
                    ""email"": ""invalid email"",
                    ""phoneNumber"": ""+447123456789""
                },
                ""deliveryMethods"": [""SMS""],
                ""documentTypes"": [""passport-scan""],
                ""serviceRequestedBy"": ""development-team-staging""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

    }
}
