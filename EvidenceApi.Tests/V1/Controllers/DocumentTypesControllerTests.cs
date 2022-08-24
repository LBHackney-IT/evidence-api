using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response.Exceptions;
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
        private Mock<IGetDocumentTypesByTeamNameUseCase> _mockGetDocumentTypesByTeamUseCase;
        private Mock<IStaffSelectedDocumentTypeGateway> _mockStaffSelectedDocumentTypeGateway;


        [SetUp]
        public void SetUp()
        {
            _mockCreateAuditUseCase = new Mock<ICreateAuditUseCase>();
            _mockGetDocumentTypesByTeamUseCase = new Mock<IGetDocumentTypesByTeamNameUseCase>();
            _mockStaffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
            _classUnderTest = new DocumentTypesController(_mockCreateAuditUseCase.Object, _mockGetDocumentTypesByTeamUseCase.Object, _mockStaffSelectedDocumentTypeGateway.Object);
        }

        [Test]
        public void ReturnsAllDocumentTypesWhenExistingTeamAndNoQuery()
        {
            // Arrange
            var docType = new DocumentType { Title = "Passport", Id = "passport" };
            var docTypes = new List<DocumentType> { docType };
            var teamName = "team";

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(teamName, null)).Returns(docTypes);

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
                new DocumentType { Title = "Proof of Status", Id = "proof-of-status", Enabled = true},
            };
            var teamName = "team";

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(teamName, true)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName, true) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsDisabledDocumentTypesWhenQueryParamFalse()
        {
            // Arrange
            var docTypes = new List<DocumentType>
            {
                new DocumentType { Title = "Proof of Status", Id = "proof-of-status", Enabled = false},
            };
            var teamName = "team";

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(teamName, false)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName, false) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsEmptyListWhenNoDocumentTypes()
        {
            // Arrange
            var docTypes = new List<DocumentType>();
            var teamName = "team";

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(teamName, null)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(teamName) as OkObjectResult;

            //Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ThrowsNotFoundWhenTeamDoesNotExist()
        {
            // Arrange
            var nonExistentTeamName = "fake";

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(nonExistentTeamName, null))
                .Throws(new NotFoundException($"No document types were found for team with name: {nonExistentTeamName}"));

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(nonExistentTeamName) as NotFoundObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<NotFoundObjectResult>();
            response?.StatusCode.Should().Be(404);
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
