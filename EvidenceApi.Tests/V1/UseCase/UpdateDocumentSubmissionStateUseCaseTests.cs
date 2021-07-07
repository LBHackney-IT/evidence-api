using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class UpdateDocumentSubmissionStateUseCaseTests
    {
        private UpdateDocumentSubmissionStateUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway = new Mock<IEvidenceGateway>();
        private Mock<IDocumentTypeGateway> _documentTypeGateway = new Mock<IDocumentTypeGateway>();
        private Mock<INotifyGateway> _notifyGateway = new Mock<INotifyGateway>();
        private Mock<IResidentsGateway> _residentsGateway = new Mock<IResidentsGateway>();
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
        private Mock<IUpdateEvidenceRequestStateUseCase> _updateEvidenceRequestStateUseCase = new Mock<IUpdateEvidenceRequestStateUseCase>();
        private Mock<ILogger<UpdateDocumentSubmissionStateUseCase>> _logger;
        private readonly IFixture _fixture = new Fixture();
        private DocumentSubmission _found;
        private Resident _resident;
        private EvidenceRequest _evidenceRequest;
        private string teamName = "teamName";

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<UpdateDocumentSubmissionStateUseCase>>();
            _classUnderTest = new UpdateDocumentSubmissionStateUseCase(
                _evidenceGateway.Object,
                _documentTypeGateway.Object,
                _notifyGateway.Object,
                _residentsGateway.Object,
                _documentsApiGateway.Object,
                _staffSelectedDocumentTypeGateway.Object,
                _updateEvidenceRequestStateUseCase.Object,
                _logger.Object);
        }

        [Test]
        public async Task ReturnsTheUpdatedDocumentSubmission()
        {
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = BuildDocumentSubmissionUpdateRequest();
            var result = await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be("UPLOADED");
        }

        [Test]
        public async Task ReturnsTheUpdatedDocumentSubmissionWhenStaffSelectedDocumentTypeIsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);

            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.StaffSelectedDocumentTypeId, "passport-scan")
                .Without(x => x.ValidUntil)
                .Create();
            _staffSelectedDocumentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), request.StaffSelectedDocumentTypeId))
                .Returns(TestDataHelper.StaffSelectedDocumentType(request.StaffSelectedDocumentTypeId));

            // Act
            var result = await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);

            // Assert
            result.Id.Should().Be(_found.Id);
            result.StaffSelectedDocumentType.Should().NotBeNull();
            result.StaffSelectedDocumentType.Id.Should().Be(_found.StaffSelectedDocumentTypeId);
            result.UserUpdatedBy.Should().Be(_found.UserUpdatedBy);
        }

        [Test]
        public async Task ReturnsTheUpdatedDocumentSubmissionWhenRejectionReasonIsProvided()
        {
            var id = Guid.NewGuid();
            SetupMocks(id, teamName);

            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.RejectionReason, "This is the rejection reason")
                .With(x => x.UserUpdatedBy, "TestEmail@hackney.gov.uk")
                .Without(x => x.ValidUntil)
                .Create();

            var result = await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);

            result.Id.Should().Be(_found.Id);
            result.RejectionReason.Should().Be(_found.RejectionReason);
            result.RejectedAt.Should().Be(_found.RejectedAt);
            result.UserUpdatedBy.Should().Be(_found.UserUpdatedBy);
        }

        [Test]
        public void ThrowsAnErrorWhenADocumentSubmissionIsNotFound()
        {
            Guid id = Guid.NewGuid();
            DocumentSubmissionUpdateRequest request = BuildDocumentSubmissionUpdateRequest();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);
            testDelegate.Should().Throw<NotFoundException>().WithMessage($"Cannot find document submission with id: {id}");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStateIsNullOrEmpty()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .Without(x => x.State)
                .Without(x => x.ValidUntil)
                .Create();

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("State in the request cannot be null");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStateIsNotValid()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Invalidstate")
                .Without(x => x.ValidUntil)
                .Create();

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("This state is invalid");
        }

        [Test]
        public void SendsANotification()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);

            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Rejected")
                .With(x => x.RejectionReason, "This is the rejection reason")
                .Without(x => x.ValidUntil)
                .Create();
            _classUnderTest.ExecuteAsync(id, request);

            _notifyGateway.Verify(x =>
                x.SendNotification(DeliveryMethod.Email, CommunicationReason.EvidenceRejected, _found, _resident));

            _notifyGateway.Verify(x =>
                x.SendNotification(DeliveryMethod.Sms, CommunicationReason.EvidenceRejected, _found, _resident));

        }

        [Test]
        public void UpdateClaim()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            Guid claimId = Guid.NewGuid();
            _found.ClaimId = claimId.ToString();

            var dateTime = DateTime.UtcNow;
            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.ValidUntil, dateTime.ToLongTimeString)
                .Create();

            // Act
            _classUnderTest.ExecuteAsync(id, request);

            // Assert
            _documentsApiGateway.Verify(x =>
                x.UpdateClaim(claimId, It.IsAny<ClaimUpdateRequest>()), Times.Exactly(1));
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenCannotUpdateClaim()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            _documentsApiGateway.Setup(x => x.UpdateClaim(It.IsAny<Guid>(), It.IsAny<ClaimUpdateRequest>()))
                .Throws(new DocumentsApiException("Issue with DocumentsApi so cannot update claim: doh!"));
            Guid claimId = Guid.NewGuid();
            _found.ClaimId = claimId.ToString();

            var dateTime = DateTime.UtcNow;
            var request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .With(x => x.ValidUntil, dateTime.ToLongTimeString)
                .Create();

            // Act
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);

            // Assert
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Issue with DocumentsApi so cannot update claim: doh!");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentSubmissionIsAccepted()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            _found.State = SubmissionState.Approved;
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "APPROVED")
                .Without(x => x.ValidUntil)
                .Create();

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Document has already been approved");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentSubmissionIsRejected()
        {
            Guid id = Guid.NewGuid();
            SetupMocks(id, teamName);
            _found.State = SubmissionState.Rejected;
            DocumentSubmissionUpdateRequest request = _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "REJECTED")
                .Without(x => x.ValidUntil)
                .Create();

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id, request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Document has already been rejected");
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
            _resident = _fixture.Create<Resident>();
            _evidenceRequest = TestDataHelper.EvidenceRequest();
            _evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms };
            _found.EvidenceRequest = _evidenceRequest;
            _found.EvidenceRequestId = _evidenceRequest.Id;

            _evidenceGateway.Setup(x => x.FindEvidenceRequest(_found.EvidenceRequestId)).Returns(_evidenceRequest);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(id)).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(ds =>
                ds.Id == id && ds.State == SubmissionState.Uploaded
            )));

            _documentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, _found.DocumentTypeId))
                .Returns(TestDataHelper.DocumentType(_found.DocumentTypeId));

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_resident).Verifiable();
            _evidenceGateway.Setup(x => x.CreateEvidenceRequest(It.IsAny<EvidenceRequest>())).Returns(_evidenceRequest).Verifiable();

            _updateEvidenceRequestStateUseCase.Setup(x => x.Execute(_found.EvidenceRequestId));

            var claim = _fixture.Create<Claim>();
            _documentsApiGateway.Setup(x => x.UpdateClaim(It.IsAny<Guid>(), It.IsAny<ClaimUpdateRequest>()))
                 .ReturnsAsync(claim).Verifiable();
        }

        private DocumentSubmissionUpdateRequest BuildDocumentSubmissionUpdateRequest()
        {
            return _fixture.Build<DocumentSubmissionUpdateRequest>()
                .With(x => x.State, "Uploaded")
                .Without(x => x.ValidUntil)
                .Create();
        }
    }
}
