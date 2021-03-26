using System;
using System.Collections.Generic;
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
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateDocumentSubmissionUseCaseTests
    {
        private CreateDocumentSubmissionUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway = new Mock<IEvidenceGateway>();
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Mock<IDocumentTypeGateway> _documentTypeGateway = new Mock<IDocumentTypeGateway>();
        private readonly IFixture _fixture = new Fixture();
        private DocumentType _documentType;
        private DocumentSubmission _created;
        private DocumentSubmissionRequest _request;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new CreateDocumentSubmissionUseCase(_evidenceGateway.Object, _documentsApiGateway.Object, _documentTypeGateway.Object);
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentTypeIsEmptyOrNull()
        {
            var documentSubmissionRequest = _fixture.Build<DocumentSubmissionRequest>()
                .Without(x => x.DocumentType)
                .Create();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(Guid.NewGuid(), documentSubmissionRequest).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ThrowsNotFoundExceptionWhenEvidenceRequestIsNull()
        {
            var request = _fixture.Create<DocumentSubmissionRequest>();
            _evidenceGateway
                .Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(x => x.DocumentTypeId == request.DocumentType)))
                .Returns(() => null)
                .Verifiable();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(Guid.NewGuid(), request).ConfigureAwait(true);
            testDelegate.Should().Throw<NotFoundException>();
        }

        [Test]
        public async Task ReturnsTheCreatedDocumentSubmissionWhenRequestIsValid()
        {
            _documentType = _fixture.Create<DocumentType>();
            _request = CreateRequestFixture();
            _created = DocumentSubmissionFixture();
            var evidenceRequest = TestDataHelper.EvidenceRequest();

            var claim = _fixture.Create<Claim>();
            var s3UploadPolicy = _fixture.Create<S3UploadPolicy>();

            SetupEvidenceGateway(evidenceRequest);
            SetupDocumentsApiGateway(evidenceRequest, claim, s3UploadPolicy);
            var docType = SetupDocumentTypeGateway(_request.DocumentType);

            var result = await _classUnderTest.ExecuteAsync(evidenceRequest.Id, _request).ConfigureAwait(true);

            result.UploadPolicy.Should().BeEquivalentTo(s3UploadPolicy);
            result.Id.Should().Be(_created.Id);
            result.ClaimId.Should().Be(_created.ClaimId);
            result.RejectionReason.Should().Be(_created.RejectionReason);
            result.State.Should().Be(_created.State.ToString().ToUpper());
            result.DocumentType.Should().Be(docType);
        }

        [TestCase(SubmissionState.Pending)]
        [TestCase(SubmissionState.Rejected)]
        public void DoesNotThrowWhenInactiveDocumentSubmissionExists(SubmissionState state)
        {
            _documentType = _fixture.Create<DocumentType>();
            _request = CreateRequestFixture();
            _created = DocumentSubmissionFixture();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var existingDocumentSubmission =
                new DocumentSubmission { State = state, DocumentTypeId = _documentType.Id };
            evidenceRequest.DocumentSubmissions = new List<DocumentSubmission> { existingDocumentSubmission };

            var claim = _fixture.Create<Claim>();
            var s3UploadPolicy = _fixture.Create<S3UploadPolicy>();

            SetupEvidenceGateway(evidenceRequest);
            SetupDocumentsApiGateway(evidenceRequest, claim, s3UploadPolicy);
            SetupDocumentTypeGateway(_request.DocumentType);

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(evidenceRequest.Id, _request).ConfigureAwait(true);
            testDelegate.Should().NotThrow();
        }

        [TestCase(SubmissionState.Approved)]
        [TestCase(SubmissionState.Uploaded)]
        public void ThrowsBadRequestIfActiveDocumentSubmissionAlreadyExists(SubmissionState state)
        {
            _documentType = _fixture.Create<DocumentType>();
            _request = CreateRequestFixture();
            _created = DocumentSubmissionFixture();
            var evidenceRequest = TestDataHelper.EvidenceRequest();
            var existingDocumentSubmission =
                new DocumentSubmission { State = state, DocumentTypeId = _documentType.Id };
            evidenceRequest.DocumentSubmissions = new List<DocumentSubmission> { existingDocumentSubmission };

            SetupEvidenceGateway(evidenceRequest);

            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(evidenceRequest.Id, _request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>();
        }

        private DocumentSubmissionRequest CreateRequestFixture()
        {
            return _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.DocumentType, _documentType.Id)
                .Create();
        }

        private DocumentSubmission DocumentSubmissionFixture()
        {
            var submission = TestDataHelper.DocumentSubmission();
            submission.DocumentTypeId = _documentType.Id;
            return submission;
        }

        private void SetupDocumentsApiGateway(EvidenceRequest evidenceRequest, Claim claim, S3UploadPolicy s3UploadPolicy)
        {
            _documentsApiGateway
                .Setup(x =>
                    x.CreateClaim(It.Is<ClaimRequest>(cr =>
                        cr.ServiceAreaCreatedBy == evidenceRequest.ServiceRequestedBy &&
                        cr.UserCreatedBy == evidenceRequest.UserRequestedBy &&
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

        private void SetupEvidenceGateway(EvidenceRequest evidenceRequest)
        {
            _evidenceGateway.Setup(x => x.FindEvidenceRequest(evidenceRequest.Id)).Returns(evidenceRequest).Verifiable();
            _evidenceGateway
                .Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(x => x.DocumentTypeId == _request.DocumentType)))
                .Returns(_created)
                .Verifiable();
        }

        private DocumentType SetupDocumentTypeGateway(string documentTypeId)
        {
            var documentType = TestDataHelper.DocumentType(documentTypeId);
            _documentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId("team", documentTypeId)).Returns(documentType);

            return documentType;
        }
    }
}
