using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;
using Notify.Exceptions;

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
                ""documentTypes"": [""proof-of-id""],
                ""team"": ""Development Housing Team"",
                ""reason"": ""staging-reason"",
                ""userRequestedBy"": ""staff@test.hackney.gov.uk"",
                ""noteToResident"": ""This is a note to resident""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(201);

            var json = await response.Content.ReadAsStringAsync();
            var created = DatabaseContext.EvidenceRequests.First();
            var resident = DatabaseContext.Residents.First();

            var formattedCreatedAt = JsonConvert.SerializeObject(created.CreatedAt);
            string expected = "{" +
                               "\"resident\":{" +
                               $"\"id\":\"{resident.Id}\"," +
                               $"\"referenceId\":\"{created.ResidentReferenceId}\"," +
                               "\"name\":\"Frodo Baggins\"," +
                               "\"email\":\"frodo@bagend.com,\"," +
                               "\"phoneNumber\":\"+447123456789\"" +
                               "}," +
                               "\"deliveryMethods\":[\"SMS\"]," +
                               "\"documentTypes\":[" +
                               "{\"id\":\"proof-of-id\",\"title\":\"Proof of ID\",\"description\":\"A valid document that can be used to prove identity\"}" +
                               "]," +
                               "\"team\":\"Development Housing Team\"," +
                               "\"reason\":\"staging-reason\"," +
                               "\"userRequestedBy\":\"staff@test.hackney.gov.uk\"," +
                               $"\"id\":\"{created.Id}\"," +
                               $"\"createdAt\":{formattedCreatedAt}," +
                               "\"documentSubmission\":null," +
                               "\"noteToResident\":\"This is a note to resident\"" +
                               "}";

            json.Should().Be(expected);

            // It sends an SMS
            MockNotifyClient.Verify(x =>
                x.SendSms("+447123456789", It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), null, null));
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
                ""team"": ""Development Housing Team""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString);
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
                    ""email"": "",
                    ""phoneNumber"": ""
                },
                ""deliveryMethods"": [""SMS""],
                ""documentTypes"": [""passport-scan""],
                ""team"": ""Development Housing Team""
            }";

            var jsonString = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(uri, jsonString);
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CanFindEvidenceRequestWithValidParams()
        {
            var resident = _fixture.Create<Resident>();
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();
            var entity = _fixture.Build<EvidenceRequest>()
                .With(x => x.ResidentId, resident.Id)
                .With(x => x.DocumentTypes, new List<string> { "passport-scan" })
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email })
                .With(x => x.Team, "Development Housing Team")
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Create();

            DatabaseContext.EvidenceRequests.Add(entity);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"api/v1/evidence_requests/{entity.Id}", UriKind.Relative);
            var responseFind = await Client.GetAsync(uri);
            var jsonFind = await responseFind.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<EvidenceRequestResponse>(jsonFind);

            var expectedDocumentType = TestDataHelper.DocumentType("passport-scan");
            var expected = entity.ToResponse(resident, new List<DocumentType> { expectedDocumentType });
            responseFind.StatusCode.Should().Be(200);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReturnNotFoundWhenCannotFindEvidenceRequest()
        {
            var fakeId = "ed0f2bd2-df90-4f01-b7f1-d30e402386d0";
            var uri = new Uri($"api/v1/evidence_requests/{fakeId}", UriKind.Relative);
            var responseFind = await Client.GetAsync(uri);
            responseFind.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task CanGetEvidenceRequestsWithValidService()
        {
            var expected = BuildEvidenceRequestsListWithSameResident();
            var team = expected[0].Team;
            var uri = new Uri($"api/v1/evidence_requests?team={team}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<EvidenceRequestResponse>>(json);

            response.StatusCode.Should().Be(200);
            result.Should().ContainSingle();
            result.Should().ContainEquivalentOf(expected[0]);
        }

        [Test]
        public async Task CanGetEvidenceRequestsWithValidServiceAndResidentId()
        {
            var expected = BuildEvidenceRequestsListWithDifferentResident();
            var team = expected[0].Team;
            var residentId = expected[0].Resident.Id;
            var uri = new Uri($"api/v1/evidence_requests?team={team}&residentId={residentId}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<EvidenceRequestResponse>>(json);

            response.StatusCode.Should().Be(200);
            result.Should().BeEquivalentTo(expected[0]);
        }

        [Test]
        public async Task DoesNotReturnEvidenceRequestsWhenParamsDoNotMatchAny()
        {
            var expected = BuildEvidenceRequestsListWithDifferentResident();
            var team = expected[0].Team;
            var residentId = expected[1].Resident.Id;
            var uri = new Uri($"api/v1/evidence_requests?team={team}&residentId={residentId}", UriKind.Relative);
            var response = await Client.GetAsync(uri);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<EvidenceRequestResponse>>(json);

            response.StatusCode.Should().Be(200);
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ReturnBadRequestWhenServiceIsEmpty()
        {
            var team = "";
            var uri = new Uri($"api/v1/evidence_requests?team={team}", UriKind.Relative);
            var response = await Client.GetAsync(uri);

            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task ReturnBadRequestWhenServiceAndResidentIdAreEmpty()
        {
            var team = "";
            var residentId = "";
            var uri = new Uri($"api/v1/evidence_requests?team={team}&residentId={residentId}", UriKind.Relative);
            var response = await Client.GetAsync(uri);

            response.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task CanSendANotificationUploadConfirmationToResidentAndStaff()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();

            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();
            evidenceRequest.ResidentId = resident.Id;
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"api/v1/evidence_requests/{evidenceRequest.Id}/confirmation", UriKind.Relative);
            var response = await Client.PostAsync(uri, null);

            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task CannotSendANotificationUploadConfirmationToResidentWhenCannotFindEvidenceRequest()
        {
            var id = Guid.NewGuid();
            var uri = new Uri($"api/v1/evidence_requests/{id}/confirmation", UriKind.Relative);
            var response = await Client.PostAsync(uri, null);
            var message = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(404);
            message.Should().Contain($"Cannot find evidence request with id: {id}");
        }

        [Test]
        public async Task CannotSendANotificationUploadConfirmationToResidentWhenCannotFindResident()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.SaveChanges();
            var uri = new Uri($"api/v1/evidence_requests/{evidenceRequest.Id}/confirmation", UriKind.Relative);
            var response = await Client.PostAsync(uri, null);
            var message = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(404);
            message.Should().Contain($"Cannot find resident with id: {evidenceRequest.ResidentId}");
        }

        [Test]
        public async Task CannotSendANotificationUploadConfirmationToResidentWhenThereIsAGovNotifyError()
        {
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var resident = TestDataHelper.Resident();
            resident.Email = "";
            resident.PhoneNumber = "";
            evidenceRequest.DeliveryMethods = new List<DeliveryMethod>() { DeliveryMethod.Email, DeliveryMethod.Sms };
            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();
            evidenceRequest.ResidentId = resident.Id;
            DatabaseContext.EvidenceRequests.Add(evidenceRequest);
            DatabaseContext.SaveChanges();

            MockNotifyClient.Setup(x =>
                x.SendEmail("", It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), null, null)).Throws(new NotifyClientException());

            var uri = new Uri($"api/v1/evidence_requests/{evidenceRequest.Id}/confirmation", UriKind.Relative);
            var response = await Client.PostAsync(uri, null);
            response.StatusCode.Should().Be(400);
        }

        private List<EvidenceRequestResponse> BuildEvidenceRequestsListWithSameResident()
        {
            var resident = TestDataHelper.Resident();

            DatabaseContext.Residents.Add(resident);
            DatabaseContext.SaveChanges();

            var documentTypes = new List<string> { "passport-scan", "drivers-licence" };
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = "Development Housing Team";
            evidenceRequest2.Team = "another-service-id";
            evidenceRequest1.ResidentId = resident.Id;
            evidenceRequest2.ResidentId = resident.Id;
            evidenceRequest1.DocumentTypes = documentTypes;
            evidenceRequest2.DocumentTypes = documentTypes;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest1);
            expected.Add(evidenceRequest2);

            return expected.ConvertAll<EvidenceRequestResponse>(er =>
            {
                var documentTypes = new List<DocumentType>();
                documentTypes.Add(TestDataHelper.DocumentType("passport-scan"));
                documentTypes.Add(TestDataHelper.DocumentType("drivers-licence"));
                return er.ToResponse(resident, documentTypes);
            });
        }
        private List<EvidenceRequestResponse> BuildEvidenceRequestsListWithDifferentResident()
        {
            var resident1 = TestDataHelper.Resident();
            var resident2 = TestDataHelper.Resident();

            DatabaseContext.Residents.Add(resident1);
            DatabaseContext.Residents.Add(resident2);
            DatabaseContext.SaveChanges();

            var documentTypes = new List<string> { "passport-scan", "drivers-licence" };
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            var evidenceRequest3 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = "Development Housing Team";
            evidenceRequest2.Team = "another-service-id";
            evidenceRequest3.Team = "yet-another-service";
            evidenceRequest1.ResidentId = resident1.Id;
            evidenceRequest2.ResidentId = resident2.Id;
            evidenceRequest3.ResidentId = resident2.Id;
            evidenceRequest1.DocumentTypes = documentTypes;
            evidenceRequest2.DocumentTypes = documentTypes;
            evidenceRequest3.DocumentTypes = documentTypes;

            DatabaseContext.EvidenceRequests.Add(evidenceRequest1);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest2);
            DatabaseContext.EvidenceRequests.Add(evidenceRequest3);
            DatabaseContext.SaveChanges();

            var expected = new List<EvidenceRequest>();

            expected.Add(evidenceRequest1);
            expected.Add(evidenceRequest2);
            expected.Add(evidenceRequest3);

            var docTypes = new List<DocumentType>();
            docTypes.Add(TestDataHelper.DocumentType("passport-scan"));
            docTypes.Add(TestDataHelper.DocumentType("drivers-licence"));

            var list = new List<EvidenceRequestResponse>();
            list.Add(evidenceRequest1.ToResponse(resident1, docTypes));
            list.Add(evidenceRequest2.ToResponse(resident2, docTypes));
            list.Add(evidenceRequest3.ToResponse(resident2, docTypes));

            return list;
        }
    }
}
