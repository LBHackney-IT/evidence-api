using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class DocumentTypesTests : IntegrationTests<Startup>
    {
        private readonly string _realTeam = "Development Housing Team";
        private readonly string _fakeTeam = "Super Duper Fake Team";
        private readonly string _invalidQuery = "blah";

        #region GetDocumentTypesByTeamName

        [Test]
        public async Task GetDocumentTypesByTeamNameReturns200()
        {
            var uri = new Uri($"api/v1/document_types/{_realTeam}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var data = JsonConvert.DeserializeObject<List<DocumentType>>(json);

            data.Should().ContainEquivalentOf(new DocumentType() { Id = "proof-of-id", Title = "Proof of ID", Description = "A valid document that can be used to prove identity", Enabled = true });
            data.Should().ContainEquivalentOf(new DocumentType() { Id = "repairs-photo", Title = "Repairs photo", Description = "A photo of the issue that need to be repaired", Enabled = true });
        }

        [Test]
        public async Task GetDocumentTypesByTeamNameReturns404WhenTeamDoesNotExist()
        {
            var uri = new Uri($"api/v1/document_types/{_fakeTeam}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            response.StatusCode.Should().Be(404);

            var json = await response.Content.ReadAsStringAsync();
            var expected = $"\"No document types were found for team with name: {_fakeTeam}\"";
            json.Should().Be(expected);
        }

        [Test]
        public async Task GetDocumentTypesByTeamNameReturns400WhenQueryParamIsInvalid()
        {
            var uri = new Uri($"api/v1/document_types/{_realTeam}?enabled={_invalidQuery}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            response.StatusCode.Should().Be(400);

            var json = await response.Content.ReadAsStringAsync();
            var expected = $"The value '{_invalidQuery}' is not valid.";
            json.Should().Contain(expected);
        }

        #endregion

        #region GetStaffSelectedDocumentTypesByTeamName

        [Test]
        public async Task GetStaffSelectedDocumentTypesByTeamNameReturns200()
        {
            var uri = new Uri($"api/v1/document_types/staff_selected/{_realTeam}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            response.StatusCode.Should().Be(200);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<DocumentType>>(json);

            data.Should().ContainEquivalentOf(new DocumentType() { Id = "passport-scan", Title = "Passport Scan", Description = "A valid passport open at the photo page", Enabled = true });
            data.Should().ContainEquivalentOf(new DocumentType() { Id = "drivers-licence", Title = "Driver's licence", Description = "A valid UK full or provisional UK driving license", Enabled = true });
        }

        [Test]
        public async Task GetStaffSelectedDocumentTypesByTeamNameReturns404WhenTeamDoesNotExist()
        {
            var uri = new Uri($"api/v1/document_types/staff_selected/{_fakeTeam}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            response.StatusCode.Should().Be(404);

            var json = await response.Content.ReadAsStringAsync();
            var expected = $"\"No staff-selected document types were found for team with name: {_fakeTeam}\"";
            json.Should().Be(expected);
        }

        [Test]
        public async Task GetStaffSelectedDocumentTypesByTeamNameReturns400WhenQueryParamIsInvalid()
        {
            var uri = new Uri($"api/v1/document_types/staff_selected/{_realTeam}?enabled={_invalidQuery}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            response.StatusCode.Should().Be(400);

            var json = await response.Content.ReadAsStringAsync();
            var expected = $"The value '{_invalidQuery}' is not valid.";
            json.Should().Contain(expected);
        }

        #endregion
    }
}
