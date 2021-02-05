using System;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.Tests.V1.Domain
{
    [TestFixture]
    public class EvidenceRequestTests
    {
        [Test]
        public void EvidenceRequestHaveCorrectAttributes()
        {
            var id = Guid.NewGuid();
            var createdAt = DateTime.Now;
            var residentId = Guid.NewGuid();
            var deliveryMethods = new List<DeliveryMethod>();
            deliveryMethods.Add(DeliveryMethod.Sms);
            var documentTypes = new List<string>();
            documentTypes.Add("passport");
            var serviceRequestedBy = "some-service";
            var userRequestedBy = "some-user";
            var communications = new List<Communication>();
            communications.Add(TestDataHelper.Communication());
            var documentSubmissions = new List<DocumentSubmission>();
            documentSubmissions.Add(TestDataHelper.DocumentSubmission());


            var evidenceRequest = new EvidenceRequest
            {
                Id = id,
                CreatedAt = createdAt,
                ResidentId = residentId,
                DeliveryMethods = deliveryMethods,
                DocumentTypes = documentTypes,
                ServiceRequestedBy = serviceRequestedBy,
                UserRequestedBy = userRequestedBy,
                Communications = communications,
                DocumentSubmissions = documentSubmissions
            };

            evidenceRequest.Id.Should().Be(id);
            evidenceRequest.CreatedAt.Should().Be(createdAt);
            evidenceRequest.ResidentId.Should().Be(residentId);
            evidenceRequest.DeliveryMethods.Should().BeEquivalentTo(deliveryMethods);
            evidenceRequest.DocumentTypes.Should().BeEquivalentTo(documentTypes);
            evidenceRequest.ServiceRequestedBy.Should().Be(serviceRequestedBy);
            evidenceRequest.UserRequestedBy.Should().Be(userRequestedBy);
            evidenceRequest.Communications.Should().BeEquivalentTo(communications);
            evidenceRequest.DocumentSubmissions.Should().BeEquivalentTo(documentSubmissions);
        }

        [Test]
        public static void CanParseDeliveryMethods()
        {
            var deliveryMethods = new List<string>();
            deliveryMethods.Add("Sms");
            var evidenceRequest = new EvidenceRequest();
            evidenceRequest.RawDeliveryMethods = deliveryMethods;

            evidenceRequest.DeliveryMethods.Should().BeOfType(typeof(List<DeliveryMethod>));
        }
    }
}
