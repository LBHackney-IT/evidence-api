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
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCaseTests
    {
        private UpdateDocumentSubmissionStateUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway = new Mock<IEvidenceGateway>();
        private Mock<IDocumentTypeGateway> _documentTypeGateway = new Mock<IDocumentTypeGateway>();
        private Mock<IUpdateEvidenceRequestStateUseCase> _updateEvidenceRequestStateUseCase = new Mock<IUpdateEvidenceRequestStateUseCase>();
        private readonly IFixture _fixture = new Fixture();
        private DocumentSubmission _found;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new UpdateDocumentSubmissionStateUseCase(_evidenceGateway.Object, _documentTypeGateway.Object, _updateEvidenceRequestStateUseCase.Object);
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmission()
        {
            var id = Guid.NewGuid();
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
            DocumentSubmissionRequest request = _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.State, "Invalidstate")
                .Create();

            Action act = () => _classUnderTest.Execute(id, request);
            act.Should().Throw<BadRequestException>().WithMessage("This state is invalid");
        }

        [Test]
        public void CallsTheGatewayWithTheCorrectParams()
        {
            _evidenceGateway.VerifyAll();
            _updateEvidenceRequestStateUseCase.VerifyAll();
        }

        private void SetupMocks(Guid id)
        {
            _found = TestDataHelper.DocumentSubmission(true);

            _evidenceGateway.Setup(x => x.FindDocumentSubmission(id)).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(ds =>
                ds.Id == id && ds.State == SubmissionState.Uploaded
            )));

            _documentTypeGateway.Setup(x => x.GetDocumentTypeById(_found.DocumentTypeId))
                .Returns(TestDataHelper.DocumentType(_found.DocumentTypeId));

            _updateEvidenceRequestStateUseCase.Setup(x => x.Execute(_found.EvidenceRequestId));
        }

        private DocumentSubmissionRequest BuildDocumentSubmissionRequest()
        {
            return _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.State, "Uploaded")
                .Create();
        }
    }
}
