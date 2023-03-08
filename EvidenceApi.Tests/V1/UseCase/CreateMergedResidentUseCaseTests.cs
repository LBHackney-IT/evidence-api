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
    public class CreateMergedResidentUseCaseTests
    {
        private CreateMergedResidentUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;
        private Mock<ILogger<CreateResidentUseCase>> _logger;
        private ResidentRequest _residentRequest;
        private Resident _resident;

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _logger = new Mock<ILogger<CreateResidentUseCase>>();
            _classUnderTest = new CreateMergedResidentUseCase(_residentsGateway.Object, _logger.Object);
        }

        [Test]
        public void CanCreateAMergedResidentWithValidParameters()
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
                PhoneNumber = "",
                Team = "some team"
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
                PhoneNumber = "070000",
                Team = "some team"
            };

            Func<ResidentResponse> testDelegate = () => _classUnderTest.Execute(request);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("'Name' must not be empty.");
        }

        [Test]
        public void AllowsCreationIfResidentAlreadyExists()
        {
            SetupMocks();
            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Resident>())).Returns(_resident).Verifiable();
            var result = _classUnderTest.Execute(_residentRequest);
            result.Name.Should().Be(_residentRequest.Name);
            result.Email.Should().Be(_residentRequest.Email);
            result.PhoneNumber.Should().Be(_residentRequest.PhoneNumber);
        }

        private void SetupMocks()
        {
            _resident = TestDataHelper.Resident();
            var team = "Fake team";
            _residentRequest = new ResidentRequest
            {
                Email = _resident.Email,
                Name = _resident.Name,
                PhoneNumber = _resident.PhoneNumber,
                Team = team
            };
            _residentsGateway.Setup(x => x.CreateResident(It.IsAny<Resident>())).Returns(_resident).Verifiable();
        }
    }
}
