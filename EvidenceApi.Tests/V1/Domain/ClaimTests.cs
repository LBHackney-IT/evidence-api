using System;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using AutoFixture;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.Tests.V1.Domain
{
    [TestFixture]
    public class ClaimTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void ClaimHasCorrectAttributes()
        {
            Guid id = Guid.NewGuid();
            Document document = _fixture.Create<Document>();
            string serviceAreaCreatedBy = "service-area";
            string userCreatedBy = "name@email";
            string apiCreatedBy = "evidence-api";
            DateTime retentionExpiresAt = DateTime.Now.AddMonths(3);

            var claim = new Claim
            {
                Id = id,
                Document = document,
                ServiceAreaCreatedBy = serviceAreaCreatedBy,
                UserCreatedBy = userCreatedBy,
                ApiCreatedBy = apiCreatedBy,
                RetentionExpiresAt = retentionExpiresAt
            };

            claim.Id.Should().Be(id);
            claim.Document.Should().Be(document);
            claim.ServiceAreaCreatedBy.Should().BeSameAs(serviceAreaCreatedBy);
            claim.UserCreatedBy.Should().BeSameAs(userCreatedBy);
            claim.ApiCreatedBy.Should().BeSameAs(apiCreatedBy);
            claim.RetentionExpiresAt.Should().Be(retentionExpiresAt);
        }
    }
}
