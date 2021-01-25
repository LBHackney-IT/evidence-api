using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCaseTests
    {
        private UpdateDocumentSubmissionStateUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private readonly IFixture _fixture = new Fixture();
        private DocumentSubmission _found;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new UpdateDocumentSubmissionStateUseCase(_evidenceGateway.Object);
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmission()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id);
            DocumentSubmissionRequest request = BuildDocumentSubmissionRequest();
            var result = _classUnderTest.Execute(id, request);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be("UPLOADED");
        }

        [Test]
        public void ThrowsAnErrorWhenADocumentSubmissionIsNotFound()
        {
            Guid id = Guid.NewGuid();
            DocumentSubmissionRequest request = BuildDocumentSubmissionRequest();
            Action act = () => _classUnderTest.Execute(id, request);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find document submission with id: {id}");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStateIsNotValid()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id);
            string invalidState = "Invalidstate";
            DocumentSubmissionRequest request = _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.State, Enum.Parse(typeof(SubmissionState), invalidState))
                .Create();
            Action act = () => _classUnderTest.Execute(id, request);
            act.Should().Throw<BadRequestException>().WithMessage("This state is invalid");
        }

        private void SetupMocks(Guid id)
        {
            _found = TestDataHelper.DocumentSubmission();

            var updated = TestDataHelper.DocumentSubmission();
            updated.Id = id;
            updated.State = SubmissionState.Uploaded;

            _evidenceGateway.Setup(x => x.FindDocumentSubmission(id)).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(ds =>
                ds.Id == id && ds.State == SubmissionState.Uploaded
            ))).Returns(updated);
        }

        private DocumentSubmissionRequest BuildDocumentSubmissionRequest()
        {
            return _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.State, "UPLOADED")
                .Create();
        }
    }
}
