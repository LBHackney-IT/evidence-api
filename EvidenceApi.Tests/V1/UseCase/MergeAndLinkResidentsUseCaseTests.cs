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
    public class MergeAndLinkResidentsUseCaseTests
    {
        private MergeAndLinkResidentsUseCase _classUnderTest;
        private Mock<CreateMergedResidentUseCase> _createMergedResidentUseCase;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<AmendClaimsGroupIdUseCase> _amendClaimsGroupIdUseCase;
        private Mock<IResidentsGateway> _residentsGateway;
        private MergeAndLinkResidentsRequest _mergeAndLinkResidentRequest;
        private Resident _finalResident;

        [SetUp]
        public void SetUp()
        {

            _createMergedResidentUseCase = new Mock<CreateMergedResidentUseCase>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _amendClaimsGroupIdUseCase = new Mock<AmendClaimsGroupIdUseCase>();
            _classUnderTest = new MergeAndLinkResidentsUseCase(_createMergedResidentUseCase.Object, _residentsGateway.Object, _evidenceGateway.Object, _amendClaimsGroupIdUseCase.Object);
        }

        [Test]
        public async void CanMergeResidentsAndReturnNewResident()
        {
            SetupMocks();
            var result = await _classUnderTest.ExecuteAsync(_mergeAndLinkResidentRequest).ConfigureAwait(true);
            result.Resident.Name.Should().Be(_mergeAndLinkResidentRequest.NewResident.Name);
            result.Resident.Email.Should().Be(_mergeAndLinkResidentRequest.NewResident.Email);
            result.Resident.PhoneNumber.Should().Be(_mergeAndLinkResidentRequest.NewResident.PhoneNumber);
        }


        private void SetupMocks()
        {
            _finalResident = TestDataHelper.Resident();
            var newGroupId = Guid.NewGuid();
            var team = "Fake team";
            var firstResidentToBeMerged = Guid.NewGuid();
            var secondResidentToBeMerged = Guid.NewGuid();
            _residentsGateway.Setup(x => x.CreateResident(It.IsAny<Resident>())).Returns(_finalResident).Verifiable();

            _mergeAndLinkResidentRequest = new MergeAndLinkResidentsRequest
            {
                Team = team,
                GroupId = newGroupId,
                NewResident =
                {
                    Email = _finalResident.Email,
                    Name = _finalResident.Name,
                    PhoneNumber = _finalResident.PhoneNumber
                },
                ResidentsToDelete = new[] { firstResidentToBeMerged, secondResidentToBeMerged }
            }
            ;
        }
    }
}
