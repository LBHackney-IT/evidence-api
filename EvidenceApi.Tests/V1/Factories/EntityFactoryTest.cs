using System;
using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public void CanMapAnEvidenceRequestDomainObjectToADatabaseEntity()
        {
            var documentTypes = new List<DocumentType> {new DocumentType() {Id = "passport", Title = "Passport"}};
            var domain = _fixture.Build<EvidenceRequest>()
                .With(x => x.DocumentTypes, documentTypes)
                .With(x => x.DeliveryMethods,
                    new List<EvidenceRequest.DeliveryMethod> {EvidenceRequest.DeliveryMethod.Email})
                .Create();

            var entity = domain.ToEntity();

            entity.ServiceRequestedBy.Should().Be(entity.ServiceRequestedBy);
            entity.DocumentTypes.Should().ContainSingle(x => x == "passport");
            entity.DeliveryMethods.Should().ContainSingle(x => x == "Email");
            entity.ResidentId.Should().Be(domain.Resident.Id);
            entity.Id.Should().Be(domain.Id);
            entity.CreatedAt.Should().Be(domain.CreatedAt);
        }

        [Test]
        public void CanMapAResidentDomainObjectToADatabaseEntity()
        {
            var domain = _fixture.Create<Resident>();

            var entity = domain.ToEntity();

            entity.Id.Should().Be(domain.Id);
            entity.CreatedAt.Should().Be(domain.CreatedAt);
            entity.Name.Should().Be(domain.Name);
            entity.Email.Should().Be(domain.Email);
            entity.PhoneNumber.Should().Be(domain.PhoneNumber);
        }
    }
}
