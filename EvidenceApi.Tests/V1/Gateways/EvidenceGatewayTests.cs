using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class EvidenceGatewayTests : DatabaseTests
    {
        private readonly IFixture _fixture = new Fixture();
        private EvidenceGateway _classUnderTest;
        private EvidenceRequest _request;
        private EvidenceRequest _created;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new EvidenceGateway(DatabaseContext);
            _request = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

            _created = _classUnderTest.CreateEvidenceRequest(_request);
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldInsertIntoTheDatabase()
        {
            var query = RecordQuery();

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.DocumentTypes.Should().BeEquivalentTo(_request.DocumentTypes.ConvertAll(x => x.Id));
            foundRecord.DeliveryMethods.Should().BeEquivalentTo(_request.DeliveryMethods.ConvertAll(x => x.ToString()));
        }

        [Test]
        public void CreatingAnEvidenceRequestShouldReturnTheCreatedRequest()
        {
            var found = RecordQuery().First();

            _created.Id.Should().Be(found.Id);
            _created.CreatedAt.Should().Be(found.CreatedAt);

        }

        private IQueryable<EvidenceRequestEntity> RecordQuery()
        {
            return DatabaseContext.EvidenceRequests.Where(x => x.ServiceRequestedBy == _request.ServiceRequestedBy);
        }
    }
}
