using System;
using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CreateResidentReferenceIdUseCaseTests
    {
        private FindOrCreateResidentReferenceIdUseCase _classUnderTest;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IStringHasher> _stringHasher;

        [SetUp]
        public void SetUp()
        {
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _stringHasher = new Mock<IStringHasher>();
            _classUnderTest = new FindOrCreateResidentReferenceIdUseCase(_evidenceGateway.Object, _stringHasher.Object);
        }

        [Test]
        public void ReturnExistingResidentReferenceId()
        {
            // Arrange
            Resident existingResident = TestDataHelper.Resident();
            Guid residentId = Guid.NewGuid();
            existingResident.Id = residentId;

            EvidenceRequest existingEvidenceRequest = TestDataHelper.EvidenceRequest();
            var evidenceRequests = new List<EvidenceRequest>() { existingEvidenceRequest };
            _evidenceGateway.Setup(x => x.FindEvidenceRequestsByResidentId(residentId)).Returns(evidenceRequests);

            // Act
            var result = _classUnderTest.Execute(existingResident);

            // Assert
            result.Should().Be(existingEvidenceRequest.ResidentReferenceId);
            _stringHasher.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void CreateNewResidentReferenceIdIfCollision()
        {
            // Arrange
            Resident existingResident = TestDataHelper.Resident();
            Guid residentId = Guid.NewGuid();
            existingResident.Id = residentId;

            EvidenceRequest existingEvidenceRequest = TestDataHelper.EvidenceRequest();
            existingEvidenceRequest.ResidentReferenceId = "ResidentRef";
            var evidenceRequests = new List<EvidenceRequest>() { existingEvidenceRequest };
            _evidenceGateway.Setup(x => x.FindEvidenceRequestsByResidentId(residentId)).Returns(new List<EvidenceRequest>());
            _evidenceGateway.Setup(x => x.GetAll()).Returns(evidenceRequests);
            _stringHasher.Setup(x => x.Create(residentId.ToString()))
                .Returns(existingEvidenceRequest.ResidentReferenceId + "123456");

            // Act
            var result = _classUnderTest.Execute(existingResident);

            // Assert
            result.Should().Be("esidentRef1");
            _stringHasher.Verify(x => x.Create(It.IsAny<string>()), Times.Exactly(1));
        }
    }
}
