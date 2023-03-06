using System;
using NUnit.Framework;
using Moq;
using FluentAssertions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using System.Threading.Tasks;



namespace EvidenceApi.Tests.V1.UseCase
{
    [TestFixture]
    public class MergeAndLinkResidentsUseCaseTests
    {
        private MergeAndLinkResidentsUseCase _classUnderTest;
        private Mock<ICreateMergedResidentUseCase> _createMergedResidentUseCase;
        private Mock<IEvidenceGateway> _evidenceGateway;
        private Mock<IAmendClaimsGroupIdUseCase> _amendClaimsGroupIdUseCase;
        private Mock<IResidentsGateway> _residentsGateway;
        private MergeAndLinkResidentsRequest _mergeAndLinkResidentRequest;
        private ResidentsTeamGroupId _firstResidentTeamGroupId;
        private ResidentsTeamGroupId _secondResidentTeamGroupId;
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Resident _finalResident;


        [SetUp]
        public void SetUp()
        {

            _createMergedResidentUseCase = new Mock<ICreateMergedResidentUseCase>();
            _residentsGateway = new Mock<IResidentsGateway>();
            _evidenceGateway = new Mock<IEvidenceGateway>();
            _amendClaimsGroupIdUseCase = new Mock<IAmendClaimsGroupIdUseCase>();
            _classUnderTest = new MergeAndLinkResidentsUseCase(_createMergedResidentUseCase.Object, _residentsGateway.Object, _evidenceGateway.Object, _amendClaimsGroupIdUseCase.Object);
        }

        [Test]
        public async Task CanMergeResidentsAndReturnNewResident()
        {
            _finalResident = TestDataHelper.ResidentWithId(Guid.NewGuid());
            var newGroupId = Guid.NewGuid();
            var team = "Fake team";
            var firstResidentToBeMerged = Guid.NewGuid();
            var secondResidentToBeMerged = Guid.NewGuid();
            _firstResidentTeamGroupId = TestDataHelper.ResidentsTeamGroupId(firstResidentToBeMerged, team);
            _secondResidentTeamGroupId = TestDataHelper.ResidentsTeamGroupId(secondResidentToBeMerged, team);
            _residentsGateway.Setup(x => x.AddResidentGroupId(firstResidentToBeMerged, team, It.IsAny<Guid>())).Returns(_firstResidentTeamGroupId.GroupId).Verifiable();
            _residentsGateway.Setup(x => x.AddResidentGroupId(secondResidentToBeMerged, team, It.IsAny<Guid>())).Returns(_secondResidentTeamGroupId.GroupId).Verifiable();
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
            };
            var result = await _classUnderTest.ExecuteAsync(_mergeAndLinkResidentRequest).ConfigureAwait(true);
            result.Resident.Name.Should().Be(_mergeAndLinkResidentRequest.NewResident.Name);
            result.Resident.Email.Should().Be(_mergeAndLinkResidentRequest.NewResident.Email);
            result.Resident.PhoneNumber.Should().Be(_mergeAndLinkResidentRequest.NewResident.PhoneNumber);
        }
    }
}
