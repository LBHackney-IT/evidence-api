using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ResidentsGatewayTests : DatabaseTests
    {
        private readonly IFixture _fixture = new Fixture();
        private ResidentsGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ResidentsGateway(DatabaseContext);
        }

        [Test]
        public async Task ReturnsTheResidentWhenItAlreadyExists()
        {
            var request = _fixture.Create<ResidentRequest>();
            var entity = new ResidentEntity()
            {
                Name = request.Name, Email = request.Email, PhoneNumber = request.PhoneNumber
            };

            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = await query.FirstAsync().ConfigureAwait(true);
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
        }

        [Test]
        public async Task CreatesAResidentWhenOneDoesNotExist()
        {
            var request = _fixture.Create<ResidentRequest>();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = await query.FirstAsync().ConfigureAwait(true);
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
        }
    }
}
