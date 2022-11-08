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
using System.Threading.Tasks;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateDocumentSubmissionWithoutEvidenceRequestUseCaseTests
    {
        private CreateDocumentSubmissionWithoutEvidenceRequestUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway = new Mock<IEvidenceGateway>();
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
        private readonly IFixture _fixture = new Fixture();
        private DocumentType _staffSelectedDocumentType;
        private DocumentSubmission _created;
        private DocumentSubmissionWithoutEvidenceRequestRequest _request;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new CreateDocumentSubmissionWithoutEvidenceRequestUseCase(_evidenceGateway.Object, _documentsApiGateway.Object, _staffSelectedDocumentTypeGateway.Object);
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenTeamIsEmptyOrNull()
        {
            var documentSubmissionWithoutEvidenceRequestRequest = _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .Without(x => x.Team)
                .Create();
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(documentSubmissionWithoutEvidenceRequestRequest);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Team is null or empty");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenUserCreatedByIsEmptyOrNull()
        {
            var documentSubmissionWithoutEvidenceRequestRequest = _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .Without(x => x.UserCreatedBy)
                .Create();
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(documentSubmissionWithoutEvidenceRequestRequest);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("UserCreatedBy is null or empty");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenStaffSelectedDocumentTypeIdIsEmptyOrNull()
        {
            var documentSubmissionWithoutEvidenceRequestRequest = _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .Without(x => x.StaffSelectedDocumentTypeId)
                .Create();
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(documentSubmissionWithoutEvidenceRequestRequest);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("StaffSelectedDocumentTypeId is null or empty");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentNameIsEmptyOrNull()
        {
            var documentSubmissionWithoutEvidenceRequestRequest = _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .Without(x => x.DocumentName)
                .Create();
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(documentSubmissionWithoutEvidenceRequestRequest);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("DocumentName is null or empty");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentDescriptionIsEmptyOrNull()
        {
            var documentSubmissionWithoutEvidenceRequestRequest = _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .Without(x => x.DocumentDescription)
                .Create();
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(documentSubmissionWithoutEvidenceRequestRequest);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("DocumentDescription is null or empty");
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenCannotCreateClaim()
        {
            // Arrange
            _staffSelectedDocumentType = _fixture.Create<DocumentType>();
            _request = CreateRequestFixture();

            _documentsApiGateway
                .Setup(x =>
                    x.CreateClaim(It.Is<ClaimRequest>(cr =>
                        cr.ServiceAreaCreatedBy == _request.Team &&
                        cr.UserCreatedBy == _request.UserCreatedBy &&
                        cr.ApiCreatedBy == "evidence_api"
                    ))
                )
                .Throws(new DocumentsApiException("error!"));

            // Act
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(_request);

            // Assert
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenCannotCreateUploadPolicy()
        {
            // Arrange
            _staffSelectedDocumentType = _fixture.Create<DocumentType>();
            _request = CreateRequestFixture();

            var claim = _fixture.Create<Claim>();
            _documentsApiGateway
                .Setup(x =>
                    x.CreateClaim(It.Is<ClaimRequest>(cr =>
                        cr.ServiceAreaCreatedBy == _request.Team &&
                        cr.UserCreatedBy == _request.UserCreatedBy &&
                        cr.ApiCreatedBy == "evidence_api"
                    ))
                )
                .ReturnsAsync(claim);

            _documentsApiGateway
                .Setup(x =>
                    x.CreateUploadPolicy(It.Is<Guid>(id =>
                        id == claim.Document.Id))
                )
                .Throws(new DocumentsApiException("error!"));

            // Act
            Func<Task<DocumentSubmissionWithoutEvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(_request);

            // Assert
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public async Task ReturnsTheCreatedDocumentSubmissionWithoutEvidenceRequestWhenRequestIsValid()
        {
            _staffSelectedDocumentType = _fixture.Create<DocumentType>();
            _created = DocumentSubmissionFixture();
            _request = CreateRequestFixture();

            var claim = _fixture.Create<Claim>();
            var s3UploadPolicy = _fixture.Create<S3UploadPolicy>();

            SetupDocumentsApiGateway(_request, claim, s3UploadPolicy);
            var staffSelectedDocumentType = SetupStaffSelectedDocumentTypeGateway(_request.StaffSelectedDocumentTypeId);
            SetupEvidenceGateway();

            var result = await _classUnderTest.ExecuteAsync(_request).ConfigureAwait(true);

            result.Id.Should().Be(_created.Id);
            result.ClaimId.Should().Be(_created.ClaimId);
            result.State.Should().Be(_created.State.ToString().ToUpper());
            result.StaffSelectedDocumentType.Should().Be(staffSelectedDocumentType);
            result.UploadPolicy.Should().BeEquivalentTo(s3UploadPolicy);
            result.Team.Should().BeEquivalentTo(_created.Team);
            result.ResidentId.Should().Be(_created.ResidentId);
        }

        private DocumentSubmissionWithoutEvidenceRequestRequest CreateRequestFixture()
        {
            return _fixture.Build<DocumentSubmissionWithoutEvidenceRequestRequest>()
                .With(x => x.StaffSelectedDocumentTypeId, _staffSelectedDocumentType.Id)
                .Create();
        }

        private DocumentSubmission DocumentSubmissionFixture()
        {
            var submission = TestDataHelper.DocumentSubmission();
            submission.Id = Guid.NewGuid();
            submission.StaffSelectedDocumentTypeId = _staffSelectedDocumentType.Id;
            return submission;
        }

        private void SetupDocumentsApiGateway(DocumentSubmissionWithoutEvidenceRequestRequest request, Claim claim, S3UploadPolicy s3UploadPolicy)
        {
            _documentsApiGateway
                .Setup(x =>
                    x.CreateClaim(It.Is<ClaimRequest>(cr =>
                        cr.ServiceAreaCreatedBy == request.Team &&
                        cr.UserCreatedBy == request.UserCreatedBy &&
                        cr.ApiCreatedBy == "evidence_api"
                    ))
                )
                .ReturnsAsync(claim);

            _documentsApiGateway
                 .Setup(x =>
                     x.CreateUploadPolicy(It.Is<Guid>(id =>
                         id == claim.Document.Id))
                 )
                 .ReturnsAsync(s3UploadPolicy);
        }

        private void SetupEvidenceGateway()
        {
            _evidenceGateway
                .Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(x => x.StaffSelectedDocumentTypeId == _request.StaffSelectedDocumentTypeId)))
                .Returns(_created)
                .Verifiable();
        }

        private DocumentType SetupStaffSelectedDocumentTypeGateway(string staffSelectedDocumentTypeId)
        {
            var staffSelectedDocumentType = TestDataHelper.StaffSelectedDocumentType(staffSelectedDocumentTypeId);
            _staffSelectedDocumentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId("team", staffSelectedDocumentTypeId)).Returns(staffSelectedDocumentType);

            return staffSelectedDocumentType;
        }
    }
}
