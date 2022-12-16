using System;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;


namespace EvidenceApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CreateResidentUseCaseTests
    {
        private CreateResidentUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;
        private Mock<ILogger<CreateResidentUseCase>> _logger;
        private ResidentRequest _residentRequest;
        private Resident _resident;

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _logger = new Mock<ILogger<CreateResidentUseCase>>();
            _classUnderTest = new CreateResidentUseCase(_residentsGateway.Object, _logger.Object);
        }

        [Test]
        public void CanCreateAResidentWithValidParameters()
        {
            SetupMocks();
            var result = _classUnderTest.Execute(_residentRequest);

            result.Name.Should().Be(_residentRequest.Name);
            result.Email.Should().Be(_residentRequest.Email);
            result.PhoneNumber.Should().Be(_residentRequest.PhoneNumber);
        }

        [Test]
        public void ThrowsValidationErrorWithoutEmailAndPhoneNumber()
        {
            var request = new ResidentRequest()
            {
                Name = "Test",
                Email = "",
                PhoneNumber = ""
            };

            Func<ResidentResponse> testDelegate = () => _classUnderTest.Execute(request);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("'Email' and 'Phone number' cannot be both empty.");
        }

        [Test]
        public void ThrowsValidationErrorWithoutName()
        {
            var request = new ResidentRequest()
            {
                Name = "",
                Email = "resident@email",
                PhoneNumber = "070000"
            };

            Func<ResidentResponse> testDelegate = () => _classUnderTest.Execute(request);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("'Name' must not be empty.");
        }

        [Test]
        public void ThrowsBadRequestIfResidentAlreadyExists()
        {
            SetupMocks();
            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Resident>())).Returns(_resident).Verifiable();

            Func<ResidentResponse> testDelegate = () => _classUnderTest.Execute(_residentRequest);

            testDelegate.Should().Throw<BadRequestException>().WithMessage("A resident with these details already exists.");
        }

        private void SetupMocks()
        {
            _resident = TestDataHelper.Resident();
            _residentRequest = new ResidentRequest
            {
                Email = _resident.Email,
                Name = _resident.Name,
                PhoneNumber = _resident.PhoneNumber
            };
            _residentsGateway.Setup(x => x.CreateResident(It.IsAny<Resident>())).Returns(_resident).Verifiable();
        }
    }
}
