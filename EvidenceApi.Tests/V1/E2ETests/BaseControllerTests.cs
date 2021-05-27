using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.E2ETests
{
    public class BaseControllerTests : IntegrationTests<Startup>
    {
        [Test]
        public async Task CanCreateAuditEventForGetRequestWithoutQueryParameters()
        {
            var documentSubmission = TestDataHelper.DocumentSubmission(true);
            DatabaseContext.DocumentSubmissions.Add(documentSubmission);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"/api/v1/document_submissions/{documentSubmission.Id}", UriKind.Relative);
            Client.DefaultRequestHeaders.Add("UserEmail", "email@email");
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var auditEvent = DatabaseContext.AuditEvents.First();
            auditEvent.UrlVisited.Should().Be($"/api/v1/document_submissions/{documentSubmission.Id}");
        }

        [Test]
        public async Task CanCreateAuditEventForGetRequestWithQueryParameters()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.SaveChanges();

            var uri = new Uri("/api/v1/evidence_requests?serviceRequestedBy=Development+Housing+Team", UriKind.Relative);
            Client.DefaultRequestHeaders.Add("UserEmail", "email@email");
            var response = await Client.GetAsync(uri).ConfigureAwait(true);
            var auditEvent = DatabaseContext.AuditEvents.First();

            auditEvent.UrlVisited.Should().Be("/api/v1/evidence_requests");
            auditEvent.HttpMethod.Should().Be("GET");
            auditEvent.RequestBody.Should().Be("?serviceRequestedBy=Development+Housing+Team");
            auditEvent.UserEmail.Should().Be("email@email");
        }

        [Test]
        public async Task CanCreateAuditEventForPostRequest()
        {
            var uri = new Uri("/api/v1/evidence_requests", UriKind.Relative);
            Client.DefaultRequestHeaders.Add("UserEmail", "email@email");
            string body = @"
            {
                ""resident"": {
                    ""name"": ""Test"",
                    ""email"": ""test@test.com,"",
                    ""phoneNumber"": ""+447123456789""
                },
                ""deliveryMethods"": [""SMS""],
                ""documentTypes"": [""proof-of-id""],
                ""serviceRequestedBy"": ""Development Housing Team"",
                ""reason"": ""test-reason"",
                ""userRequestedBy"": ""staff@test.hackney.gov.uk""
            }";
            string trimmedBody = String.Concat(body.Where(c => !Char.IsWhiteSpace(c)));
            var jsonString = new StringContent(trimmedBody, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString).ConfigureAwait(true);
            var auditEvent = DatabaseContext.AuditEvents.First();

            auditEvent.UrlVisited.Should().Be("/api/v1/evidence_requests");
            auditEvent.HttpMethod.Should().Be("POST");
            auditEvent.RequestBody.Should().Be(trimmedBody);
            auditEvent.UserEmail.Should().Be("email@email");
        }

        [Test]
        public async Task DoesNotCreateAuditEventForResidentRequests()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.SaveChanges();

            var uri = new Uri($"/api/v1/evidence_requests/{evidenceRequest.Id}", UriKind.Relative);
            Client.DefaultRequestHeaders.Add("UserEmail", "resident-dummy-value");
            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            DatabaseContext.AuditEvents.Should().BeEmpty();
        }
    }
}
