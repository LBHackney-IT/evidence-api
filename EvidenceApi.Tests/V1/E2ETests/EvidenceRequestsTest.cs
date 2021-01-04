using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using EvidenceApi.V1.Infrastructure;
using AutoFixture;
using System.Collections.Generic;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class EvidenceRequestsTest : IntegrationTests<Startup>
    {
        private readonly IFixture _fixture = new Fixture();
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
            string expected = "{" +
                               "\"resident\":{" +
                               $"\"id\":\"{resident.Id}\"," +
                               "\"name\":\"Frodo Baggins\"," +
                               "\"email\":\"frodo@bagend.com,\"," +
                               "\"phoneNumber\":\"+447123456789\"" +
                               "}," +
                               "\"deliveryMethods\":[\"SMS\"]," +
                               "\"documentTypes\":[" +
                               "{\"id\":\"passport-scan\",\"title\":\"Passport\"}" +
                               "]," +
                               "\"serviceRequestedBy\":\"development-team-staging\"," +
                               $"\"id\":\"{created.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}" +
                               "}";

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

        [Test]
        public async Task CanFindEvidenceRequestWithValidParams()
        {
            var resident = _fixture.Create<ResidentEntity>();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();
            var entity = _fixture.Build<EvidenceRequestEntity>()
                .With(x => x.ResidentId, resident.Id)
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<string> { "Email" })
                .Without(x => x.Communications)
                .Create();
            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}", UriKind.Relative);
            var formattedCreatedAt = JsonConvert.SerializeObject(entity.CreatedAt.ToDateTimeOffset());
            var responseFind = await Client.GetAsync(uri).ConfigureAwait(true);
            var jsonFind = await responseFind.Content.ReadAsStringAsync().ConfigureAwait(true);
            string expected = "{" +
                               "\"resident\":{" +
                               $"\"id\":\"{resident.Id}\"," +
                               $"\"name\":\"{resident.Name}\"," +
                               $"\"email\":\"{resident.Email}\"," +
                               $"\"phoneNumber\":\"{resident.PhoneNumber}\"" +
                               "}," +
                               "\"deliveryMethods\":[\"EMAIL\"]," +
                               "\"documentTypes\":[" +
                               "{\"id\":\"passport-scan\",\"title\":\"Passport\"}" +
                               "]," +
                               $"\"serviceRequestedBy\":\"{entity.ServiceRequestedBy}\"," +
                               $"\"id\":\"{entity.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}" +
                               "}";
            responseFind.StatusCode.Should().Be(200);
            jsonFind.Should().Be(expected);
        }

        [Test]
        public async Task ReturnNotFoundWhenCannotFindEvidenceRequest()
        {
            var fakeId = "ed0f2bd2-df90-4f01-b7f1-d30e402386d0";
            var uri = new Uri($"api/v1/evidence_requests/{fakeId}", UriKind.Relative);
            var responseFind = await Client.GetAsync(uri).ConfigureAwait(true);
            responseFind.StatusCode.Should().Be(404);
        }

    }
}
