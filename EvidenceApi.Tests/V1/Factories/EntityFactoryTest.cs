using System;
using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Factories;
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
            var domain = _fixture.Build<EvidenceRequest>()
                .With(x => x.DeliveryMethods,
                    new List<DeliveryMethod> { DeliveryMethod.Email })
                .Create();

            var entity = domain.ToEntity();

            entity.ServiceRequestedBy.Should().Be(entity.ServiceRequestedBy);
            entity.DocumentTypes.Should().BeEquivalentTo(domain.DocumentTypeIds);
            entity.DeliveryMethods.Should().ContainSingle(x => x == "Email");
            entity.ResidentId.Should().Be(domain.ResidentId);
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

        [Test]
        public void CanMapACommunicationDomainObjectToADatabaseEntity()
        {
            var domain = _fixture.Create<Communication>();

            var entity = domain.ToEntity();

            entity.Id.Should().Be(domain.Id);
            entity.CreatedAt.Should().Be(domain.CreatedAt);
            entity.Reason.Should().Be(domain.Reason.ToString());
            entity.DeliveryMethod.Should().Be(domain.DeliveryMethod.ToString());
            entity.NotifyId.Should().Be(domain.NotifyId);
            entity.TemplateId.Should().Be(domain.TemplateId);
            entity.EvidenceRequestId.Should().Be(domain.EvidenceRequestId);
        }
    }
}
