using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net.Http;
using AutoFixture;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class ResidentsTest : IntegrationTests<Startup>
    {
        [SetUp]
        public void SetUp()
        {
            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/claims/update").UsingPost()
            ).RespondWith(
                Response.Create().WithStatusCode(200).WithBody(
                    JsonConvert.SerializeObject(new List<Claim>())
                )
            );
        }
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
        public async Task SearchResidentsByTeamAndSearchQueryReturns200()
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
        public async Task SearchResidentsByGroupIdReturns200()
        {
            // Arrange
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "Development Housing Team";
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.Team = team;
            var residentGroupId = TestDataHelper.ResidentsTeamGroupId(resident.Id, team);

            DatabaseContext.Add(resident);
            DatabaseContext.Add(evidenceRequest);
            DatabaseContext.Add(residentGroupId);
            DatabaseContext.SaveChanges();

            // Act
            var uri = new Uri($"api/v1/residents/search/?groupId={residentGroupId.GroupId}", UriKind.Relative);
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
        public async Task SearchResidentsByTeamAndSearchQueryReturnsEmptyListWhenNoResidentsFound()
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
            var uri = new Uri($"api/v1/residents/search/?team={team}&searchQuery=Name=Name", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<ResidentResponse>>(json);

            data.Count.Should().Be(0);

        }

        [Test]
        public async Task SearchResidentsByGroupIdReturnsEmptyListWhenNoResidentsFound()
        {
            // Arrange
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var team = "Development Housing Team";
            evidenceRequest.ResidentId = resident.Id;
            evidenceRequest.Team = team;
            var residentGroupId = TestDataHelper.ResidentsTeamGroupId(resident.Id, team);
            var anotherGuid = "6c388e40-041e-4961-86e6-6f2abe3f1e30";

            DatabaseContext.Add(resident);
            DatabaseContext.Add(evidenceRequest);
            DatabaseContext.Add(residentGroupId);
            DatabaseContext.SaveChanges();

            // Act
            var uri = new Uri($"api/v1/residents/search/?groupId={anotherGuid}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            // Assert
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<ResidentResponse>>(json);

            data.Count.Should().Be(0);
        }



        [Test]
        public async Task CreateResidentReturns201()
        {
            string body = "{" +
                          "\"name\": \"Test Resident\"," +
                          "\"email\": \"resident@email\"," +
                          "\"phoneNumber\": \"0700000\"," +
                          "\"team\": \"some team\"" +
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
                          "\"phoneNumber\": \"\"," +
                          "\"team\": \"some team\"" +
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
                          "\"phoneNumber\": \"0700000\"," +
                          "\"team\": \"some team\"" +
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
                          "\"phoneNumber\": \"0700000\"," +
                          "\"team\": \"some team\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri($"api/v1/residents", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be("\"A resident with these details already exists.\"");
        }

        [Test]
        public async Task AmendResidentGroupIdReturns200()
        {
            var oldGroupId = Guid.NewGuid();
            var newGroupId = Guid.NewGuid();
            var resident = TestDataHelper.Resident();
            resident.Id = Guid.NewGuid();
            var team = "Development Housing Team";
            var residentTeamGroupId = TestDataHelper.ResidentsTeamGroupId(resident.Id, team);
            residentTeamGroupId.GroupId = oldGroupId;

            DatabaseContext.Residents.Add(resident);
            DatabaseContext.ResidentsTeamGroupId.Add(residentTeamGroupId);
            DatabaseContext.SaveChanges();


            string body = "{" +
                          $"\"residentId\": \"{resident.Id}\"," +
                          $"\"team\": \"{team}\"," +
                          $"\"groupId\": \"{newGroupId}\"" +
                          "}";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri("api/v1/residents/update-group-id", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<ResidentsTeamGroupId>(json);

            data.GroupId.Should().Be(newGroupId);
        }

        [Test]
        public async Task AmendResidentGroupIdReturns400WhenTeamIsNull()
        {
            string body = "{" +
                $"\"residentId\": \"{Guid.NewGuid()}\"," +
                $"\"groupId\": \"{Guid.NewGuid()}\"" +
                "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri("api/v1/residents/update-group-id", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task AmendResidentGroupIdReturns400WhenIssuesWithDocumentsApi()
        {
            DocumentsApiServer.Given(
                Request.Create().WithPath($"/api/v1/claims/update").UsingPost()
            ).RespondWith(
                Response.Create().WithStatusCode(400)
            );
            string body = "{" +
                $"\"residentId\": \"{Guid.NewGuid()}\"," +
                $"\"groupId\": \"{Guid.NewGuid()}\"" +
                "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri("api/v1/residents/update-group-id", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task AmendResidentGroupIdReturns404WhenNoRecordsFoundForResidentIdAndTeam()
        {
            string body = "{" +
                $"\"residentId\": \"{Guid.NewGuid()}\"," +
                "\"team\": \"some team\"," +
                $"\"groupId\": \"{Guid.NewGuid()}\"" +
                "}";
            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var uri = new Uri("api/v1/residents/update-group-id", UriKind.Relative);
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(404);
        }
    }
}
