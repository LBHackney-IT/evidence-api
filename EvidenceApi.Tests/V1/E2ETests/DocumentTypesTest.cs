using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class DocumentTypesTest : IntegrationTests<Startup>
    {
        [Test]
        public async Task GetDocumentTypesByTeamNameReturns200()
        {
            var uri = new Uri($"api/v1/document_types/Development Housing Team", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<DocumentType>>(json);

            data.Should().ContainEquivalentOf(new DocumentType() { Id = "proof-of-id", Title = "Proof of ID", Description = "A valid document that can be used to prove identity" });
            data.Should().ContainEquivalentOf(new DocumentType() { Id = "repairs-photo", Title = "Repairs photo", Description = "A photo of the issue that need to be repaired" });
        }
    }
}
