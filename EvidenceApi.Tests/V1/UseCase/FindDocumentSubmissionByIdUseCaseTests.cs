using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindDocumentSubmissionByIdUseCaseTests
    {
        private FindDocumentSubmissionByIdUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private readonly IFixture _fixture = new Fixture();

        private DocumentType _documentType;
        private DocumentSubmission _found;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _classUnderTest = new FindDocumentSubmissionByIdUseCase(_evidenceGateway.Object, _documentTypesGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundDocumentSubmission()
        {
            SetupMocks();
            var id = Guid.NewGuid();
            var result = _classUnderTest.Execute(id);

            //result.Id.Should().NotBeEmpty();
            result.ClaimId.Should().Be(_found.ClaimId);
            // result.DocumentTypes.Should().OnlyContain(x => x.Id == _documentType.Id);
            // result.DeliveryMethods.Should().BeEquivalentTo(_found.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()));
        }

        // [Test]
        // public void ThrowsAnErrorWhenAnEvidenceRequestIsNotFound()
        // {
        //     Guid id = new Guid();
        //     Action act = () => _classUnderTest.Execute(id);
        //     act.Should().Throw<NotFoundException>().WithMessage("Cannot retrieve evidence request");
        // }

        private void SetupMocks()
        {
            _documentType = _fixture.Create<DocumentType>();
            _found = TestDataHelper.DocumentSubmission();

            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(It.IsAny<Guid>())).Returns(_found);
        }
    }
}
