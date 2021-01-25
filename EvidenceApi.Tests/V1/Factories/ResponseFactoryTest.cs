using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public void CanMapAnEvidenceRequestDomainObjectToAResponseObject()
        {
            var documentType = new DocumentType() { Id = "passport", Title = "Passport" };

            var domain = TestDataHelper.EvidenceRequest();
            domain.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email };

            var resident = _fixture.Create<Resident>();
            var documentTypes = _fixture.Create<List<DocumentType>>();

            var response = domain.ToResponse(resident, documentTypes);

            response.ServiceRequestedBy.Should().Be(response.ServiceRequestedBy);
            response.DocumentTypes.Should().BeEquivalentTo(documentTypes);
            response.DeliveryMethods.Should().ContainSingle(x => x == "EMAIL");
            response.Resident.Should().BeEquivalentTo(resident.ToResponse());
            response.Id.Should().Be(domain.Id);
            response.CreatedAt.Should().Be(domain.CreatedAt);
        }

        [Test]
        public void CanMapAResidentDomainObjectToAResponseObject()
        {
            var domain = _fixture.Create<Resident>();

            var response = domain.ToResponse();

            response.Id.Should().Be(domain.Id);
            response.Name.Should().Be(domain.Name);
            response.Email.Should().Be(domain.Email);
            response.PhoneNumber.Should().Be(domain.PhoneNumber);
        }

        [Test]
        public void CanMapADocumentSubmissionDomainObjectToAResponseObject()
        {
            var documentType = new DocumentType() { Id = "passport", Title = "Passport" };
            var domain = TestDataHelper.DocumentSubmission();
            var s3UploadPolicy = _fixture.Create<S3UploadPolicy>();

            var response = domain.ToResponse(s3UploadPolicy);

            response.Id.Should().Be(domain.Id);
            response.CreatedAt.Should().Be(domain.CreatedAt);
            response.ClaimId.Should().Be(domain.ClaimId);
            response.RejectionReason.Should().Be(domain.RejectionReason);
            response.State.Should().Be(domain.State.ToString().ToUpper());
            response.DocumentType.Should().Be(domain.DocumentTypeId);
            response.UploadPolicy.Should().Be(s3UploadPolicy);
        }
    }
}
