using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        public void FindOrCreateReturnsTheResidentWhenItAlreadyExists()
        {
            var request = _fixture.Create<Resident>();
            var entity = request.ToEntity();

            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
        }

        [Test]
        public void FindOrCreateCreatesAResidentWhenOneDoesNotExist()
        {
            var request = _fixture.Create<Resident>();

            _classUnderTest.FindOrCreateResident(request);

            var query = DatabaseContext.Residents.Where(x => x.Name == request.Name);

            query.Count()
                .Should()
                .Be(1);

            var foundRecord = query.First();
            foundRecord.Id.Should().NotBeEmpty();
            foundRecord.Email.Should().Be(request.Email);
            foundRecord.PhoneNumber.Should().Be(request.PhoneNumber);
            foundRecord.Name.Should().Be(request.Name);
        }

        [Test]
        public void FindReturnsAResidentWhenOneExists()
        {
            var entity = _fixture.Create<ResidentEntity>();
            DatabaseContext.Residents.Add(entity);
            DatabaseContext.SaveChanges();


            var foundRecord = _classUnderTest.FindResident(entity.Id);

            foundRecord.Id.Should().Be(entity.Id);
            foundRecord.Email.Should().Be(entity.Email);
            foundRecord.PhoneNumber.Should().Be(entity.PhoneNumber);
            foundRecord.Name.Should().Be(entity.Name);
        }
    }
}
