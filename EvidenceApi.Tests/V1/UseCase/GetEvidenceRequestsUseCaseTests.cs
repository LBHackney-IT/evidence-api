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
using EvidenceApi.V1.Factories;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindEvidenceRequestsUseCaseTests
    {
        private FindEvidenceRequestsUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private Mock<IResidentsGateway> _residentsGateway;
        private readonly IFixture _fixture = new Fixture();

        private Resident _resident;
        private DocumentType _documentType;
        private List<EvidenceRequest> _found = new List<EvidenceRequest>();

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new FindEvidenceRequestsUseCase(_evidenceGateway.Object, _documentTypesGateway.Object, _residentsGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundEvidenceRequests()
        {
            SetupMocks();
            BuildEvidenceRequestsList();
            var request = new EvidenceRequestsSearchQuery()
            {
                Team = "development-team-staging"
            };

            var expected = _found.ConvertAll<EvidenceRequestResponse>(er =>
            {
                var documentTypes = er.DocumentTypes.ConvertAll<DocumentType>(dt => _documentType);
                return er.ToResponse(_resident, documentTypes);
            });

            var result = _classUnderTest.Execute(request);

            _evidenceGateway.Verify(x =>
                x.GetEvidenceRequests(request));

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ThrowsBadRequestWhenServiceIsEmpty()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                Team = ""
            };

            Action act = () => _classUnderTest.Execute(request);
            act.Should().Throw<BadRequestException>().WithMessage("Service requested by is null or empty");
        }

        private void SetupMocks()
        {
            _resident = _fixture.Create<Resident>();
            _documentType = _fixture.Create<DocumentType>();

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_resident);
            _documentTypesGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.GetEvidenceRequests(It.IsAny<EvidenceRequestsSearchQuery>())).Returns(_found);
        }

        private void BuildEvidenceRequestsList()
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.Team = "development-team-staging";
            evidenceRequest2.Team = "development-team-staging";
            _found.Add(evidenceRequest1);
            _found.Add(evidenceRequest2);
        }
    }
}
