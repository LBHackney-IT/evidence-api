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
    public class GetDocumentTypesByTeamNameUseCaseTests
    {
        private GetDocumentTypesByTeamNameUseCase _classUnderTest;
        private readonly Mock<IDocumentTypeGateway> _documentTypeGateway = new Mock<IDocumentTypeGateway>();
        private readonly string _existingTeamName = "team";

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new GetDocumentTypesByTeamNameUseCase(_documentTypeGateway.Object);
            SetUpGatewayMock();
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenTeamNameDoesNotExist()
        {
            //arrange
            _documentTypeGateway.Invocations.Clear();

            var nonExistentTeamName = "fake";
            var noDocumentTypes = new List<DocumentType>();
            _documentTypeGateway.Setup(x =>
                x.GetDocumentTypesByTeamName(nonExistentTeamName)).Returns(noDocumentTypes);

            //act
            Func<List<DocumentType>> testDelegate = () => _classUnderTest.Execute(nonExistentTeamName, null);

            //assert
            testDelegate.Should().Throw<NotFoundException>().WithMessage($"No document types were found for team with name: {nonExistentTeamName}");
        }

        [Test]
        public void ReturnsAllDocumentTypesWhenNoQueryParam()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, null);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType {Id = "proof-of-id", Title = "Proof of Id", Enabled = true},
                new DocumentType { Id = "proof-of-address", Title = "Proof of Address", Enabled = true },
                new DocumentType { Id = "proof-of-Status", Title = "Proof of Status", Enabled = false },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsEnabledDocumentTypesWhenQueryParamIsTrue()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, true);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType {Id = "proof-of-id", Title = "Proof of Id", Enabled = true},
                new DocumentType { Id = "proof-of-address", Title = "Proof of Address", Enabled = true },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        [Test]
        public void ReturnsDisabledDocumentTypesWhenQueryParamIsFalse()
        {
            //act
            var result = _classUnderTest.Execute(_existingTeamName, false);

            //assert
            var expectation = new List<DocumentType>
            {
                new DocumentType { Id = "proof-of-Status", Title = "Proof of Status", Enabled = false },
            };

            result.Should().BeEquivalentTo(expectation);
        }

        private void SetUpGatewayMock()
        {
            var allTeamDocuments = new List<DocumentType>
            {
                new DocumentType {Id = "proof-of-id", Title = "Proof of Id", Enabled = true},
                new DocumentType { Id = "proof-of-address", Title = "Proof of Address", Enabled = true },
                new DocumentType { Id = "proof-of-Status", Title = "Proof of Status", Enabled = false },
            };
            _documentTypeGateway.Setup(x =>
                x.GetDocumentTypesByTeamName(_existingTeamName)).Returns(allTeamDocuments);
        }
    }
}
