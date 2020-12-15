using System;
using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Factories
{
    [TestFixture]
    public class DomainFactoryTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void CanMapResidentEntityToDomainObject()
        {
            var entity = _fixture.Create<ResidentEntity>();

            var domain = entity.ToDomain();

            domain.Id.Should().Be(entity.Id);
            domain.CreatedAt.Should().Be(entity.CreatedAt);
            domain.Email.Should().Be(entity.Email);
            domain.Name.Should().Be(entity.Name);
            domain.PhoneNumber.Should().Be(entity.PhoneNumber);
        }

        [Test]
        public void CanMapCommunicationEntityToDomainObject()
        {
            var evidenceRequest = _fixture.Build<EvidenceRequestEntity>()
                .With(x => x.DeliveryMethods, new List<string>{ "Email" })
                .Without(x => x.Communications)
                .Create();

            var entity = _fixture.Build<CommunicationEntity>()
                .With(x => x.DeliveryMethod, "Email")
                .With(x => x.Reason, "EvidenceRejected")
                .With(x => x.EvidenceRequest, evidenceRequest)
                .Create();

            var domain = entity.ToDomain();

            domain.Id.Should().Be(entity.Id);
            domain.CreatedAt.Should().Be(entity.CreatedAt);
            domain.NotifyId.Should().Be(entity.NotifyId);
            domain.TemplateId.Should().Be(entity.TemplateId);
            domain.DeliveryMethod.Should().Be(DeliveryMethod.Email);
            domain.EvidenceRequestId.Should().Be(entity.EvidenceRequest.Id);
        }
    }
}
