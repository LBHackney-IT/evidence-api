using System;
using NUnit.Framework;
using Moq;
using AutoFixture;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Boundary.Request;
using FluentAssertions;
using System.Collections.Generic;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class EditResidentUseCaseTests
    {
        private EditResidentUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;
        private static Fixture _fixture = new Fixture();
        private Resident _foundResident;

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new EditResidentUseCase(_residentsGateway.Object);
        }

        [Test, TestCaseSource(nameof(EditResidentRequestParameterCombinations))]
        public void CanEditNameEmailPhoneNumberForResident(EditResidentRequest editResidentRequest)
        {
            SetupMocks();
            var expectedResident = new Resident()
            {
                Name = editResidentRequest.Name,
                Email = editResidentRequest.Email,
                PhoneNumber = editResidentRequest.PhoneNumber
            };
            _residentsGateway.Setup(x => x.CreateResident(It.IsAny<Resident>())).Returns(expectedResident).Verifiable();

            var result = _classUnderTest.Execute(_foundResident.Id, editResidentRequest);

            result.Id.Should().Be(expectedResident.Id);
            result.Name.Should().Be(expectedResident.Name);
            result.Email.Should().Be(expectedResident.Email);
            result.PhoneNumber.Should().Be(expectedResident.PhoneNumber);
        }

        private static IEnumerable<TestCaseData> EditResidentRequestParameterCombinations
        {
            get
            {
                yield return new TestCaseData(editResidentRequestNameEmailPhoneNumber);
                yield return new TestCaseData(editResidentRequestName);
                yield return new TestCaseData(editResidentRequestEmail);
                yield return new TestCaseData(editResidentRequestPhoneNumber);
                yield return new TestCaseData(editResidentRequestNameEmail);
                yield return new TestCaseData(editResidentRequestNamePhoneNumber);
                yield return new TestCaseData(editResidentRequestEmailPhoneNumber);
            }
        }

        private static EditResidentRequest editResidentRequestNameEmailPhoneNumber = new EditResidentRequest()
        {
            Name = "test name",
            Email = "test@email",
            PhoneNumber = "0799999"
        };

        private static EditResidentRequest editResidentRequestName = new EditResidentRequest()
        {
            Name = "test name"
        };

        private static EditResidentRequest editResidentRequestEmail = new EditResidentRequest()
        {
            Email = "test@email"
        };

        private static EditResidentRequest editResidentRequestPhoneNumber = new EditResidentRequest()
        {
            PhoneNumber = "0799999"
        };

        private static EditResidentRequest editResidentRequestNameEmail = new EditResidentRequest()
        {
            Name = "test name",
            Email = "test@email"
        };

        private static EditResidentRequest editResidentRequestNamePhoneNumber = new EditResidentRequest()
        {
            Name = "test name",
            PhoneNumber = "0799999"
        };

        private static EditResidentRequest editResidentRequestEmailPhoneNumber = new EditResidentRequest()
        {
            Email = "test@email",
            PhoneNumber = "0799999"
        };

        private void SetupMocks()
        {
            _foundResident = TestDataHelper.Resident();
            _foundResident.Id = Guid.NewGuid();

            _residentsGateway.Setup(x => x.FindResident(It.IsAny<Guid>())).Returns(_foundResident).Verifiable();
        }
    }
}
