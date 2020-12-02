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
        public async Task GetDocumentTypesReturns200()
        {
            var uri = new Uri($"api/v1/document_types", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<DocumentType>>(json);

            data.Should().ContainEquivalentOf(new DocumentType() { Id = "passport-scan", Title = "Passport" });
            data.Should().ContainEquivalentOf(new DocumentType() { Id = "bank-statement", Title = "Bank statement" });
            data.Should().ContainEquivalentOf(new DocumentType() { Id = "drivers-licence", Title = "Drivers licence" });
        }

    }
}
