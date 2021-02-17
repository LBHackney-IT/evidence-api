using System.Collections.Generic;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindResidentsBySearchQueryUseCaseTests
    {
        private FindResidentsBySearchQueryUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;
        private readonly IFixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new FindResidentsBySearchQueryUseCase(_residentsGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundResidents()
        {
            // Arrange
            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "TestResident")
                .Create();

            var searchQuery = "Test";
            _residentsGateway
                .Setup(x => x.FindResidents(searchQuery))
                .Returns(new List<Resident> { resident1 });

            // Act
            var result = _classUnderTest.Execute(searchQuery);

            // Assert
            result.Count.Should().Be(1);
            var resultResident = result.Find(r => r.Name == "TestResident");
            resultResident.Should().NotBeNull();
        }
    }
}
