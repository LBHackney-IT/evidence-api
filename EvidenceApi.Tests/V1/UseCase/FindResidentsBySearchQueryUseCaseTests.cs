using System;
using System.Collections.Generic;
using System.Text.Json;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
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
        private Mock<IEvidenceGateway> _evidenceGateway;
        private readonly IFixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new FindResidentsBySearchQueryUseCase(_residentsGateway.Object, _evidenceGateway.Object);
        }

        [Test]
        public void ReturnsTheResidentByResidentNameAndTeam()
        {
            // Arrange
            var team = "Development Housing Team";
            var request = new ResidentSearchQuery { Team = team, SearchQuery = "Test" };

            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "TestResident")
                .Create();
            var resident2 = _fixture.Build<Resident>()
                .With(x => x.Name, "TestResident")
                .Create();
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = team;
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest2.Team = "Different Team";

            _residentsGateway
                .Setup(x => x.FindResidents(request.SearchQuery))
                .Returns(new List<Resident> { resident1, resident2 });
            _evidenceGateway
                .Setup(x => x.FindEvidenceRequestsByResidentId(resident1.Id))
                .Returns(new List<EvidenceRequest> { evidenceRequest1 });
            _evidenceGateway
                .Setup(x => x.FindEvidenceRequestsByResidentId(resident2.Id))
                .Returns(new List<EvidenceRequest> { evidenceRequest2 });
            _evidenceGateway
                .Setup(x => x.GetEvidenceRequests(request))
                .Returns(new List<EvidenceRequest>());

            // Act
            var result = _classUnderTest.Execute(request);

            // Assert
            result.Count.Should().Be(2);
            var resultResident = result.Find(r => r.Name == "TestResident");
            resultResident.Should().NotBeNull();
            resultResident.Id.Should().Be(resident1.Id);
        }

        [Test]
        public void ReturnsTheResidentByResidentReferenceIdAndTeam()
        {
            // Arrange
            var team = "Development Housing Team";
            var residentReferenceId = "12345";
            var request = new ResidentSearchQuery { Team = team, SearchQuery = residentReferenceId };

            var resident1 = _fixture.Build<Resident>()
                .With(x => x.Name, "TestResident")
                .Create();
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = team;
            evidenceRequest1.ResidentReferenceId = residentReferenceId;
            evidenceRequest1.ResidentId = resident1.Id;

            _residentsGateway
                .Setup(x => x.FindResidents(request.SearchQuery))
                .Returns(new List<Resident>());
            _evidenceGateway
                .Setup(x => x.GetEvidenceRequests(request))
                .Returns(new List<EvidenceRequest> { evidenceRequest1 });
            _residentsGateway
                .Setup(x => x.FindResident(resident1.Id))
                .Returns(resident1);

            // Act
            var result = _classUnderTest.Execute(request);

            // Assert
            result.Count.Should().Be(1);
            var resultResident = result.Find(r => r.Name == "TestResident");
            resultResident.Should().NotBeNull();
            resultResident.Id.Should().Be(resident1.Id);
        }
    }
}
