using System;
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
    }
}
