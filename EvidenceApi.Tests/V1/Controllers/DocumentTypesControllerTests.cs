using System.Collections.Generic;
using EvidenceApi.V1.Controllers;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Controllers
{

    [TestFixture]
    public class DocumentTypesControllerTests
    {
        private DocumentTypesController _classUnderTest;
        private Mock<ICreateAuditUseCase> _mockCreateAuditUseCase;
        private Mock<IDocumentTypeGateway> _mockDocumentTypeGateway;
        private Mock<IStaffSelectedDocumentTypeGateway> _mockStaffSelectedDocumentTypeGateway;


        [SetUp]
        public void SetUp()
        {
            _mockCreateAuditUseCase = new Mock<ICreateAuditUseCase>();
            _mockDocumentTypeGateway = new Mock<IDocumentTypeGateway>();
            _mockStaffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
            _classUnderTest = new DocumentTypesController(_mockCreateAuditUseCase.Object, _mockDocumentTypeGateway.Object, _mockStaffSelectedDocumentTypeGateway.Object);
        }

        [Test]
        public void ReturnsAllDocumentTypes()
        {
            // Arrange
            var docType = new DocumentType { Title = "Passport", Id = "passport" };
            var docTypes = new List<DocumentType> { docType };
            var teamName = "team";

            _mockDocumentTypeGateway.Setup(s => s.GetDocumentTypesByTeamName(teamName)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsEnabledDocumentTypesWhenQueryParamTrue()
        {
            // Arrange
            var docTypes = new List<DocumentType>
            {
                new DocumentType { Title = "Proof of Address", Id = "proof-of-address", Enabled = true},
                new DocumentType { Title = "Proof of Id", Id = "proof-of-id", Enabled = true},
                new DocumentType { Title = "Proof of Status", Id = "proof-of-status", Enabled = false},
            };
            var teamName = "team";

            _mockDocumentTypeGateway.Setup(s => s.GetDocumentTypesByTeamName(teamName)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName, true) as OkObjectResult;

            // Assert
            var expectation = new List<DocumentType>
            {
                new DocumentType { Title = "Proof of Address", Id = "proof-of-address", Enabled = true},
                new DocumentType { Title = "Proof of Id", Id = "proof-of-id", Enabled = true},
            };

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsDisabledDocumentTypesWhenQueryParamFalse()
        {
            // Arrange
            var docTypes = new List<DocumentType>
            {
                new DocumentType { Title = "Proof of Address", Id = "proof-of-address", Enabled = true},
                new DocumentType { Title = "Proof of Id", Id = "proof-of-id", Enabled = true},
                new DocumentType { Title = "Proof of Status", Id = "proof-of-status", Enabled = false},
            };
            var teamName = "team";

            _mockDocumentTypeGateway.Setup(s => s.GetDocumentTypesByTeamName(teamName)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName, false) as OkObjectResult;

            // Assert
            var expectation = new List<DocumentType>
            {
                new DocumentType { Title = "Proof of Status", Id = "proof-of-status", Enabled = false},
            };

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsStaffSelectedDocumentTypes()
        {
            // Arrange
            var docType = new DocumentType { Title = "Passport", Id = "passport" };
            var docTypes = new List<DocumentType> { docType };
            var teamName = "team";

            _mockStaffSelectedDocumentTypeGateway.Setup(s => s.GetDocumentTypesByTeamName(teamName)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(teamName) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnNotFoundIfEmptyStaffSelectedDocumentTypes()
        {
            // Arrange
            var docTypes = new List<DocumentType>();
            var teamName = "team";

            _mockStaffSelectedDocumentTypeGateway.Setup(s => s.GetDocumentTypesByTeamName(teamName)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(teamName) as NotFoundObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(404);
        }
    }
}
