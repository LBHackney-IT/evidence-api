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

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new GetDocumentTypesByTeamNameUseCase(_documentTypeGateway.Object);
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenTeamNameDoesNotExist()
        {
            //arrange
            var nonExistentTeamName = "fake";
            var noDocumentTypes = new List<DocumentType>();
            _documentTypeGateway.Setup(x =>
                x.GetDocumentTypesByTeamName(nonExistentTeamName)).Returns(noDocumentTypes);
            //act
            Func<List<DocumentType>> testDelegate = () => _classUnderTest.Execute(nonExistentTeamName, null);

            //asser
            testDelegate.Should().Throw<NotFoundException>().WithMessage($"No document types were found for team with name: {nonExistentTeamName}");
        }
    }
}
