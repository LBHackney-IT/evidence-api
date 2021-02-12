using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindDocumentSubmissionByIdUseCaseTests
    {
        private FindDocumentSubmissionByIdUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private Mock<IDocumentsApiGateway> _documentsApiGateway;
        private readonly IFixture _fixture = new Fixture();

        private DocumentType _documentType;
        private DocumentSubmission _found;
        private DocumentSubmission _found2;
        private Task<Claim> _claim;
        private Task<Claim> _claim2;
        private string _claimId1 = "70cdff29-84d3-461e-bd16-2032c07c28bd";
        private string _claimId2 = "010f4156-92aa-4082-891b-3b238e46940a";
        private Guid _documentSubmissionId1 = Guid.NewGuid();
        private Guid _documentSubmissionId2 = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _documentsApiGateway = new Mock<IDocumentsApiGateway>();
            _classUnderTest = new FindDocumentSubmissionByIdUseCase(
                _evidenceGateway.Object,
                _documentTypesGateway.Object,
                _documentsApiGateway.Object
            );
        }

        [Test]
        public async Task ReturnsTheFoundDocumentSubmission()
        {
            SetupMocks();
            var result = await _classUnderTest.ExecuteAsync(_documentSubmissionId1).ConfigureAwait(true);

            result.ClaimId.Should().Be(_found.ClaimId);
            result.RejectionReason.Should().Be(_found.RejectionReason);
            result.State.Should().Be(_found.State.ToString().ToUpper());
            result.DocumentType.Should().Be(_documentType);
            result.Document.Should().NotBeNull();
        }

        [Test]
        public async Task ReturnsTheFoundDocumentSubmissionWithoutDocument()
        {
            SetupMocks();
            var result = await _classUnderTest.ExecuteAsync(_documentSubmissionId2).ConfigureAwait(true);

            result.ClaimId.Should().Be(_found2.ClaimId);
            result.RejectionReason.Should().Be(_found2.RejectionReason);
            result.State.Should().Be(_found2.State.ToString().ToUpper());
            result.DocumentType.Should().Be(_documentType);
            result.Document.Should().BeNull();
        }

        [Test]
        public void ThrowsAnErrorWhenADocumentSubmissionIsNotFound()
        {
            var id = Guid.NewGuid();
            Func<Task<DocumentSubmissionResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(id).ConfigureAwait(true);
            testDelegate.Should().Throw<NotFoundException>().WithMessage($"Cannot find document submission with ID: {id}");
        }

        private void SetupMocks()
        {
            _documentType = _fixture.Create<DocumentType>();
            _claim = _fixture.Create<Task<Claim>>();
            _claim2 = _fixture.Create<Task<Claim>>();

            _found = TestDataHelper.DocumentSubmission();
            _found.Id = _documentSubmissionId1;
            _found.ClaimId = _claimId1;

            _found2 = TestDataHelper.DocumentSubmission();
            _found2.Id = _documentSubmissionId2;
            _found2.ClaimId = _claimId2;
            _claim2.Result.Document = null;


            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(_documentSubmissionId1)).Returns(_found);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(_documentSubmissionId2)).Returns(_found2);
            _documentsApiGateway.Setup(x => x.GetClaimById(_claimId1)).Returns(_claim);
            _documentsApiGateway.Setup(x => x.GetClaimById(_claimId2)).Returns(_claim2);
        }
    }
}
