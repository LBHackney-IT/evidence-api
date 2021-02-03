using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
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

            result.ClaimId.Should().Be(_found.ClaimId);
            result.RejectionReason.Should().Be(_found.RejectionReason);
            result.State.Should().Be(_found.State.ToString().ToUpper());
            result.DocumentType.Should().Be(_documentType);

        }

        [Test]
        public void ThrowsAnErrorWhenADocumentSubmissionIsNotFound()
        {
            var id = Guid.NewGuid();
            Action act = () => _classUnderTest.Execute(id);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find document submission with ID: {id}");
        }

        private void SetupMocks()
        {
            _documentType = _fixture.Create<DocumentType>();
            _found = TestDataHelper.DocumentSubmission();

            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(It.IsAny<Guid>())).Returns(_found);
        }
    }
}
