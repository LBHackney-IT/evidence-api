using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using EvidenceApi.V1.Factories;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class GetEvidenceRequestsUseCaseTests
    {
        private GetEvidenceRequestsUseCase _classUnderTest;
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
            _classUnderTest = new GetEvidenceRequestsUseCase(_evidenceGateway.Object, _documentTypesGateway.Object, _residentsGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundEvidenceRequests()
        {
            SetupMocks();
            BuildEvidenceRequestsList();
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = "development-team-staging"
            };

            var expected = _found.ConvertAll<EvidenceRequestResponse>(er =>
            {
                var documentTypes = er.DocumentTypes.ConvertAll<DocumentType>(dt => _documentType);
                return er.ToResponse(_resident, documentTypes);
            });

            var result = _classUnderTest.Execute(request);

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ThrowsBadRequestWhenServiceIsEmpty()
        {
            var request = new EvidenceRequestsSearchQuery()
            {
                ServiceRequestedBy = ""
            };

            Action act = () => _classUnderTest.Execute(request);
            act.Should().Throw<BadRequestException>().WithMessage("Service requested by is null or empty");
        }

        private void SetupMocks()
        {
            _resident = _fixture.Create<Resident>();
            _documentType = _fixture.Create<DocumentType>();

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_resident);
            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.GetEvidenceRequests(It.IsAny<string>(), It.IsAny<Guid?>())).Returns(_found);
        }

        private void BuildEvidenceRequestsList()
        {
            var evidenceRequest1 = TestDataHelper.EvidenceRequest();
            var evidenceRequest2 = TestDataHelper.EvidenceRequest();
            evidenceRequest1.ServiceRequestedBy = "development-team-staging";
            evidenceRequest2.ServiceRequestedBy = "development-team-staging";
            _found.Add(evidenceRequest1);
            _found.Add(evidenceRequest2);
        }
    }
}
