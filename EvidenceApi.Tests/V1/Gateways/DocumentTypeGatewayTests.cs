using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DocumentTypeGatewayTests
    {
        private DocumentTypeGateway _classUnderTest;
        private Mock<IFileReader<List<Team>>> _fileReaderMock;

        [SetUp]
        public void Setup()
        {
            _fileReaderMock = new Mock<IFileReader<List<Team>>>();
            _classUnderTest = new DocumentTypeGateway(_fileReaderMock.Object);
        }

        [Test]
        public void GetDocumentTypeByTeamNameAndDocumentIdReturnsTheDocumentTypeIfItExists()
        {
            // Arrange
            var docType = new DocumentType { Id = "passport", Title = "Passport" };
            var docTypes = new List<DocumentType> { docType };
            var teamName = "teamName";
            var team = new Team();
            team.Name = teamName;
            team.DocumentTypes = docTypes;
            var teams = new List<Team> { team };
            _fileReaderMock.Setup(s => s.GetData()).Returns(teams);

            // Act
            var response = _classUnderTest.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, docType.Id);

            // Assert
            response.Id.Should().BeSameAs(docType.Id);
            response.Title.Should().BeSameAs(docType.Title);
        }

        [Test]
        public void GetDocumentTypeByTeamNameAndDocumentIdNullIfDocumentTypeDoesNotExist()
        {
            _fileReaderMock.Setup(s => s.GetData()).Returns(new List<Team>());
            var response = _classUnderTest.GetDocumentTypeByTeamNameAndDocumentTypeId("teamName", "passport");

            response.Should().BeNull();
        }

        [Test]
        public void GetDocumentTypesByTeamReturnsDocumentTypesIfTeamExists()
        {
            // Arrange
            var docType = new DocumentType { Id = "passport", Title = "Passport" };
            var docTypes = new List<DocumentType> { docType };
            var teamName = "teamName";
            var team = new Team();
            team.Name = teamName;
            team.DocumentTypes = docTypes;
            var teams = new List<Team> { team };
            _fileReaderMock.Setup(s => s.GetData()).Returns(teams);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName);

            // Assert
            response.Should().Contain(docTypes);
        }

        [Test]
        public void GetDocumentTypesByTeamReturnsEmptyListIfTeamDoesNotExist()
        {
            // Arrange
            var teamName = "teamName";
            var team = new Team();
            team.Name = teamName;
            var teams = new List<Team> { team };
            _fileReaderMock.Setup(s => s.GetData()).Returns(teams);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName("differentTeamName");

            // Assert
            response.Should().BeEmpty();
        }

        [Test]
        public void GetTeamIdByTeamNameReturnsTeamIdIfTeamNameDoesNotExist()
        {
            // Arrange
            var teamName = "teamName";
            var team = new Team();
            team.Name = teamName;
            var teams = new List<Team> { team };
            _fileReaderMock.Setup(s => s.GetData()).Returns(teams);

            // Act
            var response = _classUnderTest.GetTeamIdByTeamName("differentTeamName");

            // Assert
            response.Should().BeEmpty();
        }

        [Test]
        public void GetTeamIdByTeamNameReturnsTeamIdIfTeamExists()
        {
            // Arrange
            var teamId = "1";
            var teamName = "teamName";
            var team = new Team();
            team.Name = teamName;
            team.Id = teamId;
            var teams = new List<Team> { team };
            _fileReaderMock.Setup(s => s.GetData()).Returns(teams);

            // Act
            var response = _classUnderTest.GetTeamIdByTeamName(teamName);

            // Assert
            response.Should().Contain(teamId);
        }
    }
}
