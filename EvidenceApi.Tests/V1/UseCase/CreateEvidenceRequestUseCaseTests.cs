using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateEvidenceRequestUseCaseTests
    {
        private CreateEvidenceRequestUseCase _classUnderTest;
        private Mock<IEvidenceRequestValidator> _validator;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IDocumentTypeGateway> _documentTypesGateway;
        private Mock<IResidentsGateway> _residentsGateway;
        private Mock<INotifyGateway> _notifyGateway;
        private readonly IFixture _fixture = new Fixture();

        private Resident _resident;
        private DocumentType _documentType;
        private EvidenceRequest _created;
        private EvidenceRequestRequest _request;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<IEvidenceRequestValidator>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _documentTypesGateway = new Mock<IDocumentTypeGateway>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _notifyGateway = new Mock<INotifyGateway>();
            _classUnderTest = new CreateEvidenceRequestUseCase(_validator.Object, _documentTypesGateway.Object,
                _residentsGateway.Object, _evidenceGateway.Object, _notifyGateway.Object);

        }

        [Test]
        public void ThrowsBadRequestErrorWhenRequestIsInvalid()
        {
            SetupValidatorToReturn(false);
            Func<EvidenceRequestResponse> testDelegate = () => _classUnderTest.Execute(new EvidenceRequestRequest());
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void ReturnsTheCreatedRecordWhenRequestIsValid()
        {
            SetupValidatorToReturn(true);
            SetupMocks();
            var result = _classUnderTest.Execute(_request);

            result.Resident.Id.Should().NotBeEmpty();
            result.Resident.Name.Should().Be(_resident.Name);
            result.DocumentTypes.Should().OnlyContain(x => x.Id == _documentType.Id);
            result.DeliveryMethods.Should().BeEquivalentTo(_created.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()));
        }

        [Test]
        public void CreatesTheResident()
        {
            SetupValidatorToReturn(true);
            SetupMocks();

            _classUnderTest.Execute(_request);

            _residentsGateway.VerifyAll();
        }

        [Test]
        public void SendsANotification()
        {
            SetupValidatorToReturn(true);
            SetupMocks();

            _classUnderTest.Execute(_request);

            _notifyGateway.Verify(x =>
                x.SendNotification(DeliveryMethod.Email, CommunicationReason.EvidenceRequest, _created, _resident));

            _notifyGateway.Verify(x =>
                x.SendNotification(DeliveryMethod.Sms, CommunicationReason.EvidenceRequest, _created, _resident));

        }

        private void SetupValidatorToReturn(bool valid)
        {
            var result = new Mock<ValidationResult>();
            result.Setup(x => x.IsValid).Returns(valid);
            _validator.Setup(x => x.Validate(It.IsAny<EvidenceRequestRequest>()))
                .Returns(result.Object);
        }

        private void SetupMocks()
        {
            _resident = _fixture.Create<Resident>();
            _documentType = _fixture.Create<DocumentType>();
            _created = _fixture.Build<EvidenceRequest>()
                .With(x => x.DeliveryMethods, new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms })
                .Create();

            _request = _fixture.Build<EvidenceRequestRequest>()
                .With(x => x.DeliveryMethods, new List<string> { "EMAIL" })
                .Create();

            _residentsGateway.Setup(x => x.FindOrCreateResident(It.IsAny<ResidentRequest>())).Returns(_resident);
            _documentTypesGateway.Setup(x => x.GetDocumentTypeById(It.IsAny<string>())).Returns(_documentType);
            _evidenceGateway.Setup(x => x.CreateEvidenceRequest(It.IsAny<EvidenceRequest>())).Returns(_created);
        }
    }
}
