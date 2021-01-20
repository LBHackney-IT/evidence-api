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
    public class CreateDocumentSubmissionUseCaseTests
    {
        private CreateDocumentSubmissionUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentsApiGateway> _documentsApiGateway;
        private readonly IFixture _fixture = new Fixture();
        private DocumentType _documentType;
        private DocumentSubmission _created;
        private DocumentSubmissionRequest _request;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentsApiGateway = new Mock<IDocumentsApiGateway>();
            _classUnderTest = new CreateDocumentSubmissionUseCase(_evidenceGateway.Object, _documentsApiGateway.Object);
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenDocumentTypeIsEmptyOrNull()
        {
            var documentSubmissionRequest = _fixture.Build<DocumentSubmissionRequest>()
                .Without(x => x.DocumentType)
                .Create();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(new Guid(), documentSubmissionRequest).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenServiceNameIsEmptyOrNull()
        {
            var documentSubmissionRequest = _fixture.Build<DocumentSubmissionRequest>()
                .Without(x => x.ServiceName)
                .Create();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(new Guid(), documentSubmissionRequest).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ThrowsBadRequestExceptionWhenRequesterEmailIsEmptyOrNull()
        {
            var documentSubmissionRequest = _fixture.Build<DocumentSubmissionRequest>()
                .Without(x => x.RequesterEmail)
                .Create();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(new Guid(), documentSubmissionRequest).ConfigureAwait(true);
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
            var evidenceRequest = CreateEvidenceRequestFixture();
            var claim = _fixture.Create<Claim>();
            var s3UploadPolicy = _fixture.Create<S3UploadPolicy>();

            SetupEvidenceGateway(evidenceRequest);
            SetupDocumentsApiGateway(claim, s3UploadPolicy);

            var result = await _classUnderTest.ExecuteAsync(evidenceRequest.Id, _request).ConfigureAwait(true);

            result.UploadPolicy.Should().BeEquivalentTo(s3UploadPolicy);
            result.Id.Should().NotBeEmpty();
            result.ClaimId.Should().Be(_created.ClaimId);
            result.RejectionReason.Should().Be(_created.RejectionReason);
            result.State.Should().Be(_created.State.ToString().ToUpper());
            result.DocumentType.Should().BeEquivalentTo(_created.DocumentTypeId);
        }

        private DocumentSubmissionRequest CreateRequestFixture()
        {
            return _fixture.Build<DocumentSubmissionRequest>()
                .With(x => x.DocumentType, _documentType.ToString())
                .Create();
        }

        private DocumentSubmission DocumentSubmissionFixture()
        {
            return _fixture.Build<DocumentSubmission>()
                .With(x => x.DocumentTypeId, _documentType.ToString())
                .Create();
        }

        private EvidenceRequest CreateEvidenceRequestFixture()
        {
            return _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Create();
        }

        private void SetupDocumentsApiGateway(Claim claim, S3UploadPolicy s3UploadPolicy)
        {
            _documentsApiGateway
                .Setup(x =>
                    x.CreateClaim(It.Is<ClaimRequest>(cr =>
                        cr.ServiceAreaCreatedBy == _request.ServiceName &&
                        cr.UserCreatedBy == _request.RequesterEmail &&
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
    }
}
