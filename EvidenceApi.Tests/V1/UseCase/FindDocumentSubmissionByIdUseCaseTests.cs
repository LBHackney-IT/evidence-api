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
        private Task<Claim> _claim;
        private string _id = "70cdff29-84d3-461e-bd16-2032c07c28bd";

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
            var id = Guid.NewGuid();
            var result = await _classUnderTest.ExecuteAsync(id).ConfigureAwait(true);

            result.ClaimId.Should().Be(_found.ClaimId);
            result.RejectionReason.Should().Be(_found.RejectionReason);
            result.State.Should().Be(_found.State.ToString().ToUpper());
            result.DocumentType.Should().Be(_documentType);
            result.Document.Should().NotBeNull();
        }

        // test: return the found document subnmission with no document
        // [Test]
        // public async Task ReturnsTheFoundDocumentSubmissionWithoutDocument()
        // {
        //     SetupMocks();
        //     var id = Guid.NewGuid();
        //     var result = await _classUnderTest.ExecuteAsync(id).ConfigureAwait(true);

        //     result.ClaimId.Should().Be(_found.ClaimId);
        //     result.RejectionReason.Should().Be(_found.RejectionReason);
        //     result.State.Should().Be(_found.State.ToString().ToUpper());
        //     result.DocumentType.Should().Be(_documentType);
        //     result.Document.Should().BeNull();
        // }

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
            _found = TestDataHelper.DocumentSubmission();
            _found.ClaimId = _id;

            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(It.IsAny<Guid>())).Returns(_found);
            _documentsApiGateway.Setup(x => x.GetClaimById(_id)).Returns(_claim);
        }
    }
}
