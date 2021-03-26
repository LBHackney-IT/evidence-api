using System;
using AutoFixture;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;
using System.Collections.Generic;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindDocumentSubmissionsByResidentIdUseCaseTests
    {
        private FindDocumentSubmissionsByResidentIdUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway;
        private Mock<IDocumentsApiGateway> _documentsApiGateway;
        private readonly IFixture _fixture = new Fixture();
        private DocumentType _documentType;
        private EvidenceRequest _evidenceRequest1;
        private EvidenceRequest _evidenceRequest2;
        private DocumentSubmissionSearchQuery _useCaseRequest;
        private DocumentSubmission _documentSubmission1;
        private DocumentSubmission _documentSubmission2;
        private List<DocumentSubmission> _found;
        private Task<Claim> _claim1;
        private Task<Claim> _claim2;
        private string _claimId1 = "70cdff29-84d3-461e-bd16-2032c07c28bd";
        private string _claimId2 = "010f4156-92aa-4082-891b-3b238e46940a";


        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
            _documentsApiGateway = new Mock<IDocumentsApiGateway>();
            _classUnderTest = new FindDocumentSubmissionsByResidentIdUseCase(
                _evidenceGateway.Object,
                _documentTypesGateway.Object,
                _staffSelectedDocumentTypeGateway.Object,
                _documentsApiGateway.Object
            );
        }

        [Test]
        public async Task ReturnsTheFoundDocumentSubmissions()
        {
            SetupMocks();

            var result = await _classUnderTest.ExecuteAsync(_useCaseRequest).ConfigureAwait(true);

            var documentSubmission1 = result[0];
            var documentSubmission2 = result[1];

            documentSubmission1.Id.Should().Be(_documentSubmission1.Id);
            documentSubmission1.ClaimId.Should().BeEquivalentTo(_documentSubmission1.ClaimId);
            documentSubmission1.DocumentType.Should().BeEquivalentTo(_documentType);
            documentSubmission1.StaffSelectedDocumentType.Should().BeEquivalentTo(_documentType);
            documentSubmission1.State.Should().BeEquivalentTo(_documentSubmission1.State.ToString());
            documentSubmission1.RejectionReason.Should().BeEquivalentTo(_documentSubmission1.RejectionReason);
            documentSubmission2.Id.Should().Be(_documentSubmission2.Id);
            documentSubmission2.ClaimId.Should().BeEquivalentTo(_documentSubmission2.ClaimId);
            documentSubmission2.DocumentType.Should().BeEquivalentTo(_documentType);
            documentSubmission2.StaffSelectedDocumentType.Should().BeEquivalentTo(_documentType);
            documentSubmission2.State.Should().BeEquivalentTo(_documentSubmission2.State.ToString());
            documentSubmission2.RejectionReason.Should().BeEquivalentTo(_documentSubmission2.RejectionReason);
        }

        [Test]
        public void ThrowsBadRequestWhenServiceRequestedByIsEmpty()
        {
            var serviceRequestedBy = "";
            var residentId = Guid.NewGuid();
            var request = new DocumentSubmissionSearchQuery()
            {
                ServiceRequestedBy = serviceRequestedBy,
                ResidentId = residentId
            };
            Func<Task<List<DocumentSubmissionResponse>>> testDelegate = async () => await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Service requested by is null or empty");
        }

        [Test]
        public void ThrowsBadRequestWhenResidentIdIsEmpty()
        {
            var serviceRequestedBy = "some service";
            Guid residentId = Guid.Empty;
            var request = new DocumentSubmissionSearchQuery()
            {
                ServiceRequestedBy = serviceRequestedBy,
                ResidentId = residentId
            };
            Func<Task<List<DocumentSubmissionResponse>>> testDelegate = async () => await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Resident ID is invalid");
        }

        private void SetupMocks()
        {
            var residentId = Guid.NewGuid();
            _evidenceRequest1 = TestDataHelper.EvidenceRequest();
            _evidenceRequest1.Id = Guid.NewGuid();
            _evidenceRequest1.ServiceRequestedBy = "Housing benefit";
            _evidenceRequest1.ResidentId = residentId;

            _evidenceRequest2 = TestDataHelper.EvidenceRequest();
            _evidenceRequest2.Id = Guid.NewGuid();
            _evidenceRequest2.ServiceRequestedBy = "Housing benefit";
            _evidenceRequest2.ResidentId = residentId;

            _documentSubmission1 = TestDataHelper.DocumentSubmission();
            _documentSubmission1.EvidenceRequestId = _evidenceRequest1.Id;
            _documentSubmission1.ClaimId = _claimId1;

            _documentSubmission2 = TestDataHelper.DocumentSubmission();
            _documentSubmission2.EvidenceRequestId = _evidenceRequest1.Id;
            _documentSubmission2.ClaimId = _claimId2;

            _documentType = _fixture.Create<DocumentType>();

            _claim1 = _fixture.Create<Task<Claim>>();

            _claim2 = _fixture.Create<Task<Claim>>();
            _claim2.Result.Document = null;

            _useCaseRequest = new DocumentSubmissionSearchQuery()
            {
                ServiceRequestedBy = "Housing benefit",
                ResidentId = residentId
            };

            var evidenceRequestsResult = new List<EvidenceRequest>()
            {
                _evidenceRequest1, _evidenceRequest2
            };

            _found = new List<DocumentSubmission>()
            {
                _documentSubmission1, _documentSubmission2
            };

            _documentTypesGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), It.IsAny<string>())).Returns(_documentType);
            _staffSelectedDocumentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.GetEvidenceRequests(It.IsAny<EvidenceRequestsSearchQuery>())).Returns(evidenceRequestsResult);
            _evidenceGateway.Setup(x => x.FindDocumentSubmissionByEvidenceRequestId(_evidenceRequest1.Id)).Returns(_found);
            _evidenceGateway.Setup(x => x.FindDocumentSubmissionByEvidenceRequestId(_evidenceRequest2.Id)).Returns(new List<DocumentSubmission>());
            _documentsApiGateway.Setup(x => x.GetClaimById(_claimId1)).Returns(_claim1);
            _documentsApiGateway.Setup(x => x.GetClaimById(_claimId2)).Returns(_claim2);
        }
    }
}
