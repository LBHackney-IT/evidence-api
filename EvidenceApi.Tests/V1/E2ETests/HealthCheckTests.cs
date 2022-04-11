using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class HealthCheckTests : IntegrationTests<Startup>
    {
        [Test]
        public async Task HealthCheckErrorReturns500()
        {
            var uri = new Uri($"api/v1/healthcheck/error", UriKind.Relative);

            var expected = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occured while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            var response = await Client.GetAsync(uri);
            var body = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<ProblemDetails>(body);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
