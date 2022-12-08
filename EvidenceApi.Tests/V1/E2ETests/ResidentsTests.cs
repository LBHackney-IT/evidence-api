using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net.Http;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class ResidentsTest : IntegrationTests<Startup>
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public async Task GetResidentReturns200()
        {
            var resident = TestDataHelper.Resident();
            DatabaseContext.Add(resident);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"api/v1/residents/{resident.Id}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<ResidentResponse>(json);

            data.Should().BeEquivalentTo(resident, opts => opts.Excluding(x => x.CreatedAt));
        }

        [Test]
        public async Task SearchResidentsReturns200()
        {
            // Arrange
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "Development Housing Team";
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.Team = team;

            DatabaseContext.Add(resident);
            DatabaseContext.Add(evidenceRequest);
            DatabaseContext.SaveChanges();

            // Act
            var uri = new Uri($"api/v1/residents/search/?team={team}&searchQuery={resident.Name}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<ResidentResponse>>(json);

            data.Count.Should().Be(1);
            data.Find(r => r.Name == resident.Name)
                .Should().BeEquivalentTo(resident, opts => opts.Excluding(x => x.CreatedAt));
        }

        [Test]
        public async Task CreateResidentReturns201()
        {
            string body = "{" +
                          "\"name\": \"Test Resident\"," +
                          "\"email\": \"resident@email\"," +
                          "\"phoneNumber\": \"0700000\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/residents", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(201);
        }

        [Test]
        public async Task CreateResidentReturns400WhenEmailAndPhoneNumberAreMissing()
        {
            string body = "{" +
                          "\"name\": \"Test Resident\"," +
                          "\"email\": \"\"," +
                          "\"phoneNumber\": \"\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/residents", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be("\"'Email' and 'Phone number' cannot be both empty.\"");
        }

        [Test]
        public async Task CreateResidentReturns400WhenNameIsMissing()
        {
            string body = "{" +
                          "\"name\": \"\"," +
                          "\"email\": \"resident@email\"," +
                          "\"phoneNumber\": \"0700000\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/residents", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be("\"'Name' must not be empty.\"");
        }

        [Test]
        public async Task CreateResidentReturns400IfResidentAlreadyExists()
        {
            var resident = _fixture.Build<Resident>()
                .With(x => x.Name, "Test Resident")
                .With(x => x.Email, "resident@email")
                .With(x => x.PhoneNumber, "0700000")
                .Create();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            string body = "{" +
                          "\"name\": \"Test Resident\"," +
                          "\"email\": \"resident@email\"," +
                          "\"phoneNumber\": \"0700000\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/residents", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be("\"A resident with these details already exists.\"");
        }
    }
}
