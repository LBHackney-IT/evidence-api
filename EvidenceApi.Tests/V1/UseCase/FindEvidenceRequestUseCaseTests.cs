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

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindEvidenceREquestUseCaseTests
    {
        private FindEvidenceRequestUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private Mock<IResidentsGateway> _residentsGateway;
        private readonly IFixture _fixture = new Fixture();

        private Resident _resident;
        private DocumentType _documentType;
        private EvidenceRequest _found;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new FindEvidenceRequestUseCase(_evidenceGateway.Object, _documentTypesGateway.Object, _residentsGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundEvidenceRequest()
        {
            SetupMocks();
            Guid id = new Guid();
            var result = _classUnderTest.Execute(id);

            result.Resident.Id.Should().NotBeEmpty();
            result.Resident.Name.Should().Be(_resident.Name);
            result.DocumentTypes.Should().OnlyContain(x => x.Id == _documentType.Id);
            result.DeliveryMethods.Should().BeEquivalentTo(_found.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()));
        }

        [Test]
        public void ThrowsAnErrorWhenAnEvidenceRequestIsNotFound()
        {
            Guid id = new Guid();
            Action act = () => _classUnderTest.Execute(id);
            act.Should().Throw<NotFoundException>().WithMessage("Cannot retrieve evidence request");
        }

        private void SetupMocks()
        {
            _resident = _fixture.Create<Resident>();
            _documentType = _fixture.Create<DocumentType>();
            _found = _fixture.Create<EvidenceRequest>();

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_resident);
            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.FindEvidenceRequest(It.IsAny<Guid>())).Returns(_found);
        }
    }
}