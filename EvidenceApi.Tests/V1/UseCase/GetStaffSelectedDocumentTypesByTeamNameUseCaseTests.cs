using System;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class GetStaffSelectedDocumentTypesByTeamNameUseCaseTests
    {
        private GetStaffSelectedDocumentTypesByTeamNameUseCase _classUnderTest;
        private readonly Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
        private readonly string _existingTeamName = "team";

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new GetStaffSelectedDocumentTypesByTeamNameUseCase(_staffSelectedDocumentTypeGateway.Object);
            SetUpGatewayMock();
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenTeamNameDoesNotExist()
        {
            //arrange
            _staffSelectedDocumentTypeGateway.Invocations.Clear();

            var nonExistentTeamName = "fake";
            var noDocumentTypes = new List<DocumentType>();
            _staffSelectedDocumentTypeGateway.Setup(x =>
                x.GetDocumentTypesByTeamName(nonExistentTeamName)).Returns(noDocumentTypes);

            //act
            Func<List<DocumentType>> testDelegate = () => _classUnderTest.Execute(nonExistentTeamName, null);

            //assert
            testDelegate.Should().Throw<NotFoundException>().WithMessage($"No staff-selected document types were found for team with name: {nonExistentTeamName}");
        }

        [Test]
        public void ReturnsAllStaffSelectedDocumentTypesWhenNoQueryParam()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, null);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType {Id = "passport-scan", Title = "Passport Scan", Description = "this is a description", Enabled = true},
                new DocumentType { Id = "drivers-licence", Title = "Driver's Licence", Description = "this is a description",  Enabled = true },
                new DocumentType { Id = "biometric-card", Title = "Biometric Card", Description = "this is a description", Enabled = false },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsEnabledStaffSelectedDocumentTypesWhenQueryParamIsTrue()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, true);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType {Id = "passport-scan", Title = "Passport Scan", Description = "this is a description", Enabled = true},
                new DocumentType { Id = "drivers-licence", Title = "Driver's Licence", Description = "this is a description",  Enabled = true },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsDisabledStaffSelectedDocumentTypesWhenQueryParamIsFalse()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, false);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType { Id = "biometric-card", Title = "Biometric Card", Description = "this is a description", Enabled = false },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        private void SetUpGatewayMock()
        {
            var allTeamDocuments = new List<DocumentType>
            {
                new DocumentType {Id = "passport-scan", Title = "Passport Scan", Description = "this is a description", Enabled = true},
                new DocumentType { Id = "drivers-licence", Title = "Driver's Licence", Description = "this is a description",  Enabled = true },
                new DocumentType { Id = "biometric-card", Title = "Biometric Card", Description = "this is a description", Enabled = false },
            };
            _staffSelectedDocumentTypeGateway.Setup(x =>
                x.GetDocumentTypesByTeamName(_existingTeamName)).Returns(allTeamDocuments);
        }


    }
}
