using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class ResidentsTest : IntegrationTests<Startup>
    {
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
    }
}
