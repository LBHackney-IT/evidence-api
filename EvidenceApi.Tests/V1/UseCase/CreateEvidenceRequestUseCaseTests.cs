using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
using Microsoft.Extensions.Logging;

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
        private Mock<IFindOrCreateResidentReferenceIdUseCase> _createResidentReferenceIdUseCase;
        private Mock<ILogger<CreateEvidenceRequestUseCase>> _logger;
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
            _createResidentReferenceIdUseCase = new Mock<IFindOrCreateResidentReferenceIdUseCase>();
            _logger = new Mock<ILogger<CreateEvidenceRequestUseCase>>();
            _classUnderTest = new CreateEvidenceRequestUseCase(_validator.Object, _documentTypesGateway.Object,
                _residentsGateway.Object, _evidenceGateway.Object, _notifyGateway.Object, _createResidentReferenceIdUseCase.Object, _logger.Object);
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
            result.Team.Should().Be(_created.Team);
            result.Reason.Should().Be(_created.Reason);
            result.UserRequestedBy.Should().Be(_created.UserRequestedBy);
        }

        [Test]
        public void CallsGatewaysUsingCorrectDomainObjects()
        {
            SetupValidatorToReturn(true);
            SetupMocks();

            _classUnderTest.Execute(_request);

            _residentsGateway.Verify(x => x.FindOrCreateResident(
                It.Is<Resident>(r =>
                    r.Name == _resident.Name &&
                    r.Email == _resident.Email &&
                    r.PhoneNumber == _resident.PhoneNumber
                )
            ));

            _evidenceGateway.Verify(x => x.CreateEvidenceRequest(
                It.Is<EvidenceRequest>(e =>
                    e.ResidentId == _resident.Id &&
                    e.Team == _request.Team &&
                    e.UserRequestedBy == _request.UserRequestedBy
                )
            ));
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
            _created = TestDataHelper.EvidenceRequest();
            _created.DeliveryMethods = new List<DeliveryMethod> { DeliveryMethod.Email, DeliveryMethod.Sms };

            var residentRequest = new ResidentRequest
            {
                Email = _resident.Email,
                Name = _resident.Name,
                PhoneNumber = _resident.PhoneNumber
            };
            _request = _fixture.Build<EvidenceRequestRequest>()
                .With(x => x.DeliveryMethods, new List<string> { "EMAIL" })
                .With(x => x.Resident, residentRequest)
                .Create();

            _documentTypesGateway.Setup(x => x.GetDocumentTypeByTeamNameAndDocumentTypeId(It.IsAny<string>(), It.IsAny<string>())).Returns(_documentType);
            _residentsGateway.Setup(x => x.FindOrCreateResident(It.IsAny<Resident>())).Returns(_resident).Verifiable();
            _evidenceGateway.Setup(x => x.CreateEvidenceRequest(It.IsAny<EvidenceRequest>())).Returns(_created).Verifiable();
        }
    }
}
