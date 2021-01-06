using System;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using NUnit.Framework;
using AutoFixture;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.Tests.V1.Domain
{
    [TestFixture]
    public class DocumentSubmissionTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void DocumentSubmissionHaveCorrectAttributes()
        {
            Guid id = Guid.NewGuid();
            string claimId = "0001";
            string rejectionReason = "blurry";
            SubmissionState state = SubmissionState.pending;
            EvidenceRequest evidenceRequest = _fixture.Create<EvidenceRequest>();
            string documentTypeId = "passport-scan";

            var documentSubmission = new DocumentSubmission { Id = id, ClaimId = claimId, RejectionReason = rejectionReason, State = state, EvidenceRequest = evidenceRequest, DocumentTypeId = documentTypeId };

            documentSubmission.Id.Should().Be(id);
            documentSubmission.ClaimId.Should().BeSameAs(claimId);
            documentSubmission.RejectionReason.Should().BeSameAs(rejectionReason);
            documentSubmission.State.Should().Be(state);
            documentSubmission.EvidenceRequest.Should().Be(evidenceRequest);
            documentSubmission.DocumentTypeId.Should().BeSameAs(documentTypeId);
        }
    }
}
