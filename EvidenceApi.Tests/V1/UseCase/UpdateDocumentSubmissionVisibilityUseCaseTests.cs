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
    public class UpdateDocumentSubmissionVisibilityUseCaseTests
    {
        private UpdateDocumentSubmissionVisibilityUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway = new Mock<IEvidenceGateway>();
        private Mock<IDocumentTypeGateway> _documentTypeGateway = new Mock<IDocumentTypeGateway>();
        private Mock<INotifyGateway> _notifyGateway = new Mock<INotifyGateway>();
        private Mock<IResidentsGateway> _residentsGateway = new Mock<IResidentsGateway>();
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Mock<IStaffSelectedDocumentTypeGateway> _staffSelectedDocumentTypeGateway = new Mock<IStaffSelectedDocumentTypeGateway>();
        private Mock<IUpdateEvidenceRequestStateUseCase> _updateEvidenceRequestStateUseCase = new Mock<IUpdateEvidenceRequestStateUseCase>();
        private Mock<ILogger<UpdateDocumentSubmissionVisibilityUseCase>> _logger;
        private readonly IFixture _fixture = new Fixture();
        private DocumentSubmission _found;
        private Resident _resident;
        private EvidenceRequest _evidenceRequest;
        private string _teamName = "teamName";

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<UpdateDocumentSubmissionVisibilityUseCase>>();
            _classUnderTest = new UpdateDocumentSubmissionVisibilityUseCase(
                _documentTypeGateway.Object,
                _evidenceGateway.Object,
                _logger.Object);
        }

        [Test]
        public void ReturnsTheUpdatedDocumentSubmission()
        {
            var id = Guid.NewGuid();
            SetupMocks(id, _teamName);
            DocumentSubmissionVisibilityUpdateRequest request = BuildDocumentSubmissionVisibilityUpdateRequest(true);
            var result = _classUnderTest.ExecuteAsync(id, request);
            result.Id.Should().Be(_found.Id);
            result.IsHidden.Should().BeTrue();
        }


        private void SetupMocks(Guid id, string teamName)
        {
            _found = TestDataHelper.DocumentSubmission(true);
            _resident = _fixture.Create<Resident>();
            _evidenceRequest = TestDataHelper.EvidenceRequest();
            _evidenceRequest.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms };
            _found.EvidenceRequest = _evidenceRequest;
            _found.EvidenceRequestId = _evidenceRequest.Id;

            _evidenceGateway.Setup(x => x.FindEvidenceRequest((Guid) _found.EvidenceRequestId)).Returns(_evidenceRequest);
            _evidenceGateway.Setup(x => x.FindDocumentSubmission(id)).Returns(_found);
            _evidenceGateway.Setup(x => x.CreateDocumentSubmission(It.Is<DocumentSubmission>(ds =>
                ds.Id == id && ds.State == SubmissionState.Uploaded
            )));

            _documentTypeGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(teamName, _found.DocumentTypeId))
                .Returns(TestDataHelper.DocumentType(_found.DocumentTypeId));

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_resident).Verifiable();
            _evidenceGateway.Setup(x => x.CreateEvidenceRequest(It.IsAny<EvidenceRequest>())).Returns(_evidenceRequest).Verifiable();

            _updateEvidenceRequestStateUseCase.Setup(x => x.Execute((Guid) _found.EvidenceRequestId));

            var claim = _fixture.Create<Claim>();
            _documentsApiGateway.Setup(x => x.UpdateClaim(It.IsAny<Guid>(), It.IsAny<ClaimUpdateRequest>()))
                 .ReturnsAsync(claim).Verifiable();
        }

        private DocumentSubmissionVisibilityUpdateRequest BuildDocumentSubmissionVisibilityUpdateRequest(bool visibility)
        {
            return _fixture.Build<DocumentSubmissionVisibilityUpdateRequest>()
                .With(x => x.DocumentHidden, visibility)
                .Create();
        }
    }
}
