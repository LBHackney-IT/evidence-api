using System;
using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
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
            var documentType = new DocumentType() {Id = "passport", Title = "Passport"};

            var documentTypes = new List<DocumentType> {new DocumentType() {Id = "passport", Title = "Passport"}};
            var domain = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, documentTypes)
                .With(x => x.DeliveryMethods,
                    new List<EvidenceRequest.DeliveryMethod> {EvidenceRequest.DeliveryMethod.Email})
                .Create();

            var response = domain.ToResponse();

            var resident = new ResidentResponse()
            {
                Name = domain.Resident.Name,
                Email = domain.Resident.Email,
                PhoneNumber = domain.Resident.PhoneNumber,
                Id = domain.Resident.Id
            };

            response.ServiceRequestedBy.Should().Be(response.ServiceRequestedBy);
            response.DocumentTypes.Should().ContainSingle(x => x == "passport");
            response.DeliveryMethods.Should().ContainSingle(x => x == "EMAIL");
            response.Resident.Should().BeEquivalentTo(resident);
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
    }
}
