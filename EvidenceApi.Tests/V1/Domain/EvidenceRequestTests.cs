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

        [Test]
        public void CanDetermineApprovedState()
        {
            var documentTypes = new List<String>();
            documentTypes.Add("passport-scan");
            documentTypes.Add("drivers-licence");

            var documentSubmissions = new List<DocumentSubmission>();

            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.State = SubmissionState.Approved;

            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.DocumentTypeId = "drivers-licence";
            documentSubmission2.State = SubmissionState.Rejected;

            var documentSubmission3 = TestDataHelper.DocumentSubmission();
            documentSubmission3.DocumentTypeId = "drivers-licence";
            documentSubmission3.State = SubmissionState.Approved;

            documentSubmissions.Add(documentSubmission1);
            documentSubmissions.Add(documentSubmission2);
            documentSubmissions.Add(documentSubmission3);

            var evidenceRequest = new EvidenceRequest()
            {
                DocumentTypes = documentTypes,
                DocumentSubmissions = documentSubmissions
            };

            evidenceRequest.State().Should().Be(EvidenceRequestState.Approved);
        }

        [Test]
        public void CanDeterminePendingState()
        {
            var documentTypes = new List<String>();
            documentTypes.Add("passport-scan");
            documentTypes.Add("drivers-licence");

            var documentSubmissions = new List<DocumentSubmission>();

            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.State = SubmissionState.Approved;

            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.DocumentTypeId = "drivers-licence";
            documentSubmission2.State = SubmissionState.Pending;

            var documentSubmission3 = TestDataHelper.DocumentSubmission();
            documentSubmission3.DocumentTypeId = "bank-statement";
            documentSubmission3.State = SubmissionState.Rejected;

            documentSubmissions.Add(documentSubmission1);
            documentSubmissions.Add(documentSubmission2);
            documentSubmissions.Add(documentSubmission3);


            var evidenceRequest = new EvidenceRequest()
            {
                DocumentTypes = documentTypes,
                DocumentSubmissions = documentSubmissions
            };

            evidenceRequest.State().Should().Be(EvidenceRequestState.Pending);
        }
    }
}
