using System;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Boundary.Response.Exceptions;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class UpdateEvidenceRequestStateUseCaseTests
    {
        private UpdateEvidenceRequestStateUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private EvidenceRequest _found;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _classUnderTest = new UpdateEvidenceRequestStateUseCase(_evidenceGateway.Object);
        }

        [Test]
        public void ReturnsTheUpdatedEvidenceRequestWhenStateIsApproved()
        {
            var id = Guid.NewGuid();
            SetupMocks();
            EvidenceRequestApprovedState(_found);
            var result = _classUnderTest.Execute(id);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be(EvidenceRequestState.Approved);
        }

        [Test]
        public void ReturnsTheUpdatedEvidenceRequestWhenStateIsForReview()
        {
            var id = Guid.NewGuid();
            SetupMocks();
            EvidenceRequestForReviewState(_found);
            var result = _classUnderTest.Execute(id);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be(EvidenceRequestState.ForReview);
        }

        [Test]
        public void ReturnsTheUpdatedEvidenceRequestWhenStateIsPending()
        {
            var id = Guid.NewGuid();
            SetupMocks();
            EvidenceRequestForPendingState(_found);
            var result = _classUnderTest.Execute(id);

            result.Id.Should().Be(_found.Id);
            result.State.Should().Be(EvidenceRequestState.ForReview);
        }

        [Test]
        public void ThrowsAnErrorWhenAnEvidenceRequestIsNotFound()
        {
            var id = Guid.NewGuid();
            Action act = () => _classUnderTest.Execute(id);
            act.Should().Throw<NotFoundException>().WithMessage($"Cannot find an evidence request with ID: {id}");
        }

        [Test]
        public void CallsTheGatewayWithTheCorrectParams()
        {
            _evidenceGateway.VerifyAll();
        }

        private void SetupMocks()
        {
            _found = TestDataHelper.EvidenceRequest();

            _evidenceGateway.Setup(x => x.FindEvidenceRequest(It.IsAny<Guid>())).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateEvidenceRequest(It.IsAny<EvidenceRequest>())).Returns(_found);
        }

        private static void EvidenceRequestApprovedState(EvidenceRequest evidenceRequest)
        {
            evidenceRequest.DocumentTypes.Clear();
            evidenceRequest.DocumentTypes.Add("passport-scan");
            evidenceRequest.DocumentTypes.Add("bank-statement");
            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.State = SubmissionState.Approved;
            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.DocumentTypeId = "bank-statement";
            documentSubmission2.State = SubmissionState.Approved;
            evidenceRequest.DocumentSubmissions.Add(documentSubmission1);
            evidenceRequest.DocumentSubmissions.Add(documentSubmission2);
        }

        private static void EvidenceRequestForReviewState(EvidenceRequest evidenceRequest)
        {
            evidenceRequest.DocumentTypes.Clear();
            evidenceRequest.DocumentTypes.Add("passport-scan");
            evidenceRequest.DocumentTypes.Add("bank-statement");
            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.State = SubmissionState.Approved;
            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.DocumentTypeId = "bank-statement";
            documentSubmission2.State = SubmissionState.Uploaded;
            evidenceRequest.DocumentSubmissions.Add(documentSubmission1);
            evidenceRequest.DocumentSubmissions.Add(documentSubmission2);
        }

        private static void EvidenceRequestForPendingState(EvidenceRequest evidenceRequest)
        {
            evidenceRequest.DocumentTypes.Clear();
            evidenceRequest.DocumentTypes.Add("passport-scan");
            evidenceRequest.DocumentTypes.Add("bank-statement");
            var documentSubmission1 = TestDataHelper.DocumentSubmission();
            documentSubmission1.DocumentTypeId = "passport-scan";
            documentSubmission1.State = SubmissionState.Uploaded;
            var documentSubmission2 = TestDataHelper.DocumentSubmission();
            documentSubmission2.DocumentTypeId = "bank-statement";
            documentSubmission2.State = SubmissionState.Pending;
            evidenceRequest.DocumentSubmissions.Add(documentSubmission1);
            evidenceRequest.DocumentSubmissions.Add(documentSubmission2);
        }
    }
}
