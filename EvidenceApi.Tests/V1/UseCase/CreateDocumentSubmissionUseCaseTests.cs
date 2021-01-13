using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateDocumentSubmissionUseCaseTests
    {
        private CreateDocumentSubmissionUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private readonly IFixture _fixture = new Fixture();
        private DocumentType _documentType;
        private DocumentSubmission _created;
        private DocumentSubmissionRequest _request;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new CreateDocumentSubmissionUseCase(_evidenceGateway.Object);
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentTypeIsEmptyOrNull()
        {
            var _documentSubmissionRequest = _fixture.Build<DocumentSubmissionRequest>()
                .Without(x => x.DocumentType)
                .Create();
            Func<DocumentSubmissionResponse> testDelegate = () => _classUnderTest.Execute(new Guid(), _documentSubmissionRequest);
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenEvidenceRequestIsNull()
        {
            var request = _fixture.Create<DocumentSubmissionRequest>();
            _evidenceGateway
                .Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(x => x.DocumentTypeId == request.DocumentType)))
                .Returns(() => null);
            Func<DocumentSubmissionResponse> testDelegate = () => _classUnderTest.Execute(Guid.NewGuid(), request);
            testDelegate.Should().Throw<NotFoundException>();
        }

        [Test]
        public void ReturnsTheCreatedDocumentSubmissionWhenRequestIsValid()
        {
            _documentType = _fixture.Create<DocumentType>();
            _request = _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.DocumentType, _documentType.ToString())
                .Create();
            _created = _fixture.Build<DocumentSubmission>()
                .With(x => x.DocumentTypeId, _documentType.ToString())
                .Create();
            var evidenceRequest = _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Create();
            _evidenceGateway.Setup(x => x.FindEvidenceRequest(evidenceRequest.Id)).Returns(evidenceRequest);
            _evidenceGateway
                .Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(x => x.DocumentTypeId == _request.DocumentType)))
                .Returns(_created);

            var result = _classUnderTest.Execute(evidenceRequest.Id, _request);

            result.Id.Should().NotBeEmpty();
            result.ClaimId.Should().Be(_created.ClaimId);
            result.RejectionReason.Should().Be(_created.RejectionReason);
            result.State.Should().Be(_created.State.ToString());
            result.DocumentType.Should().BeEquivalentTo(_created.DocumentTypeId);
        }
    }
}
