using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Controllers;
using EvidenceApi.V1.Domain;
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
        private Mock<IGetStaffSelectedDocumentTypesByTeamNameUseCase>
            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase;
        private readonly string _fakeTeam = "The best team everr";
        private readonly string _realTeam = "Generic Team Name";



        [SetUp]
        public void SetUp()
        {
            _mockCreateAuditUseCase = new Mock<ICreateAuditUseCase>();
            _mockGetDocumentTypesByTeamUseCase = new Mock<IGetDocumentTypesByTeamNameUseCase>();
            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase =
                new Mock<IGetStaffSelectedDocumentTypesByTeamNameUseCase>();
            _classUnderTest = new DocumentTypesController(_mockCreateAuditUseCase.Object, _mockGetDocumentTypesByTeamUseCase.Object, _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Object);
        }

        #region GetDocumentTypesByTeamName

        [Test]
        public void ReturnsAllDocumentTypesWhenExistingTeamAndNoQuery()
        {
            // Arrange
            var docType = new DocumentType { Title = "Passport", Id = "passport" };
            var docTypes = new List<DocumentType> { docType };

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(_realTeam, null)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(_realTeam) as OkObjectResult;

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
            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(_realTeam, true)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(_realTeam, true) as OkObjectResult;

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

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(_realTeam, false)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(_realTeam, false) as OkObjectResult;

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

            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(_realTeam, null)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(_realTeam) as OkObjectResult;

            //Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void DocumentTypesThrowsNotFoundWhenTeamDoesNotExist()
        {
            // Arrange
            _mockGetDocumentTypesByTeamUseCase.Setup(s => s.Execute(_fakeTeam, null))
                .Throws(new NotFoundException($"No document types were found for team with name: {_fakeTeam}"));

            // Act
            var response = _classUnderTest.GetDocumentTypesByTeamName(_fakeTeam) as NotFoundObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.Value.Should().Be($"No document types were found for team with name: {_fakeTeam}");
            response.Should().BeOfType<NotFoundObjectResult>();
            response?.StatusCode.Should().Be(404);
        }

        #endregion

        #region GetStaffSelectedDocumentTypesByTeamName

        [Test]
        public void ReturnsAllStaffSelectedDocumentTypesWhenExistingTeamAndNoQuery()
        {
            // Arrange
            var docType = new DocumentType { Title = "Passport Scan", Id = "passport-scan", Description = "A valid passport open at the photo page", Enabled = true };
            var docTypes = new List<DocumentType> { docType };

            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Setup(s => s.Execute(_realTeam, null)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(_realTeam) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsEnabledStaffSelectedDocumentTypesWhenQueryParamTrue()
        {
            // Arrange
            var docTypes = new List<DocumentType>
            {
                new DocumentType { Title = "Passport Scan", Id = "passport-scan", Description = "A valid passport open at the photo page", Enabled = true },
                new DocumentType { Title = "Driver's licence", Id = "drivers-licence", Description = "A valid UK full or provisional UK driving license", Enabled = true },
            };
            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Setup(s => s.Execute(_realTeam, true)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(_realTeam, true) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsDisabledStaffSelectedDocumentTypesWhenQueryParamFalse()
        {
            // Arrange
            var docTypes = new List<DocumentType>
            {
                new DocumentType { Title = "Passport Scan", Id = "passport-scan", Description = "A valid passport open at the photo page", Enabled = false },
            };

            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Setup(s => s.Execute(_realTeam, false)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(_realTeam, false) as OkObjectResult;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void ReturnsEmptyListWhenNoStaffSelectedDocumentTypes()
        {
            // Arrange
            var docTypes = new List<DocumentType>();

            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Setup(s => s.Execute(_realTeam, null)).Returns(docTypes);

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(_realTeam) as OkObjectResult;

            //Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }

        [Test]
        public void StaffSelectedDocumentTypesThrowsNotFoundWhenTeamDoesNotExist()
        {
            // Arrange
            _mockGetStaffSelectedDocumentTypesByTeamNameUseCase.Setup(s => s.Execute(_fakeTeam, null))
                .Throws(new NotFoundException($"No staff-selected document types were found for team with name: {_fakeTeam}"));

            // Act
            var response = _classUnderTest.GetStaffSelectedDocumentTypesByTeamName(_fakeTeam) as NotFoundObjectResult;

            // Assert
            response.Should().NotBeNull();
            response?.Value.Should().Be($"No staff-selected document types were found for team with name: {_fakeTeam}");
            response.Should().BeOfType<NotFoundObjectResult>();
            response?.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
