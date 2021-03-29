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
        private Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
        private Mock<IUpdateEvidenceRequestStateUseCase> _updateEvidenceRequestStateUseCase = new Mock<IUpdateEvidenceRequestStateUseCase>();
        private readonly IFixture _fixture = new Fixture();
        private DocumentSubmission _found;
        private string teamName = "teamName";

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new UpdateDocumentSubmissionStateUseCase(
                _evidenceGateway.Object,
                _documentTypeGateway.Object,
                _staffSelectedDocumentTypeGateway.Object,
                _updateEvidenceRequestStateUseCase.Object);
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmission()
        {
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = BuildDocumentSubmissionUpdateRequest();
            var result = _classUnderTest.Execute(id, request);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be("UPLOADED");
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmissionWhenStaffSelectedDocumentTypeIsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);

            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.StaffSelectedDocumentTypeId, "passport-scan")
                .Create();
            _staffSelectedDocumentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), request.StaffSelectedDocumentTypeId))
                .Returns(TestDataHelper.StaffSelectedDocumentType(request.StaffSelectedDocumentTypeId));

            // Act
            var result = _classUnderTest.Execute(id, request);

            // Assert
            result.Id.Should().Be(_found.Id);
            result.StaffSelectedDocumentType.Should().NotBeNull();
            result.StaffSelectedDocumentType.Id.Should().Be(_found.StaffSelectedDocumentTypeId);
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmissionWhenRejectionReasonIsProvided()
        {
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);

            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.RejectionReason, "This is the rejection reason")
                .Create();

            var result = _classUnderTest.Execute(id, request);

            result.Id.Should().Be(_found.Id);
            result.RejectionReason.Should().Be("This is the rejection reason");
        }

        [Test]
        public void ThrowsAnErrorWhenADocumentSubmissionIsNotFound()
        {
            Guid id = Guid.NewGuid();
            DocumentSubmissionUpdateRequest request = BuildDocumentSubmissionUpdateRequest();
            Action act = () => _classUnderTest.Execute(id, request);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find document submission with id: {id}");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStateIsNullOrEmpty()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .Without(x => x.State)
                .Create();

            Action act = () => _classUnderTest.Execute(id, request);
            act.Should().Throw<BadRequestException>().WithMessage("State in the request cannot be null");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStateIsNotValid()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
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

        private void SetupMocks(Guid id, string teamName)
        {
            _found = TestDataHelper.DocumentSubmission(true);

            var evidenceRequest = TestDataHelper.EvidenceRequest();
            _evidenceGateway.Setup(x => x.FindEvidenceRequest(_found.EvidenceRequestId)).Returns(evidenceRequest);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(id)).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(ds =>
                ds.Id == id && ds.State == SubmissionState.Uploaded
            )));

            _documentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, _found.DocumentTypeId))
                .Returns(TestDataHelper.DocumentType(_found.DocumentTypeId));

            _updateEvidenceRequestStateUseCase.Setup(x => x.Execute(_found.EvidenceRequestId));
        }

        private DocumentSubmissionUpdateRequest BuildDocumentSubmissionUpdateRequest()
        {
            return _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .Create();
        }
    }
}
