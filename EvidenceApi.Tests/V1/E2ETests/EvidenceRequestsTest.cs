using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
using FluentAssertions;
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
                ""documentType"": ""passport-scan"",
                ""serviceRequestedBy"": ""development-team-staging""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<EvidenceRequestResponse>(json);

            data.Id.Should().BeOfType<string>();
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
                ""documentType"": ""passport-scan"",
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
                ""documentType"": ""passport-scan"",
                ""serviceRequestedBy"": ""development-team-staging""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

    }
}
