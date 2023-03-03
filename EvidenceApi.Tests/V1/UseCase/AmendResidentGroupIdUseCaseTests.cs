using System;
using NUnit.Framework;
using Moq;
using AutoFixture;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using System.Collections.Generic;

namespace EvidenceApi.Tests.V1.UseCase
{
    [TestFixture]
    public class AmendResidentGroupIdUseCaseTests
    {
        private AmendClaimsGroupIdUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway = new Mock<IResidentsGateway>();
        private Mock<IDocumentsApiGateway> _documentsApiGateway = new Mock<IDocumentsApiGateway>();
        private Mock<ILogger<AmendClaimsGroupIdUseCase>> _logger = new Mock<ILogger<AmendClaimsGroupIdUseCase>>();
        private readonly IFixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new AmendClaimsGroupIdUseCase(_residentsGateway.Object, _documentsApiGateway.Object, _logger.Object);
        }

        [Test]
        public void ThrowsBadRequestIfTeamIsNull()
        {
            var residentTeamGroupId = _fixture.Build<ResidentGroupIdRequest>()
                .Without(x => x.Team)
                .Create();
            Func<Task<bool>> testDelegate = async () => await _classUnderTest.Execute(residentTeamGroupId);
            testDelegate.Should().Throw<BadRequestException>().WithMessage("Team must not be null");
        }

        [Test]
        public void ThrowsNotFoundWhenCannotFindResidentTeamGroupIdByResidentIdAndTeam()
        {
            _residentsGateway
                .Setup(x =>
                    x.FindResidentTeamGroupIdByResidentIdAndTeam(It.IsAny<Guid>(), It.IsAny<string>())
                )
                .Returns(() => null);
            var residentTeamGroupId = _fixture.Build<ResidentGroupIdRequest>()
                .Create();

            Func<Task<bool>> testDelegate = async () => await _classUnderTest.Execute(residentTeamGroupId);

            testDelegate.Should().Throw<NotFoundException>().WithMessage("No record found for that residentId and team");
        }

        [Test]
        public void ThrowsBadRequestWhenCannotThereAreIssuesWithDocumentsApi()
        {
            _residentsGateway
                .Setup(x =>
                    x.FindResidentTeamGroupIdByResidentIdAndTeam(It.IsAny<Guid>(), It.IsAny<string>())
                )
                .Returns(new ResidentsTeamGroupId());
            _documentsApiGateway
                .Setup(x =>
                    x.UpdateClaimsGroupId(It.IsAny<ClaimsUpdateRequest>())
                )
                .Throws(new DocumentsApiException("error!"));
            var residentTeamGroupId = _fixture.Build<ResidentGroupIdRequest>()
                .Create();

            Func<Task<bool>> testDelegate = async () => await _classUnderTest.Execute(residentTeamGroupId);

            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void CanAmendResidentGroupId()
        {
            var oldGroupId = Guid.NewGuid();
            var newGroupId = Guid.NewGuid();
            var residentId = Guid.NewGuid();
            var team = "some team";
            var residentGroupIdRequest = _fixture.Build<ResidentGroupIdRequest>()
                .With(x => x.ResidentId, residentId)
                .With(x => x.Team, team)
                .With(x => x.GroupId, newGroupId)
                .Create();
            var residentTeamGroupId = _fixture.Build<ResidentsTeamGroupId>()
                .With(x => x.GroupId, oldGroupId)
                .With(x => x.ResidentId, residentId)
                .With(x => x.Team, team)
                .Create();
            var updatedResidentTeamGroupId = residentTeamGroupId;
            updatedResidentTeamGroupId.GroupId = newGroupId;
            _residentsGateway
                .Setup(x =>
                    x.FindResidentTeamGroupIdByResidentIdAndTeam(residentGroupIdRequest.ResidentId, residentGroupIdRequest.Team)
                )
                .Returns(residentTeamGroupId);
            _residentsGateway
                .Setup(x =>
                    x.UpdateResidentGroupId(residentTeamGroupId.ResidentId, residentTeamGroupId.Team, residentTeamGroupId.GroupId)
                )
                .Returns(updatedResidentTeamGroupId);
            _documentsApiGateway
                .Setup(x =>
                    x.UpdateClaimsGroupId(It.Is<ClaimsUpdateRequest>(x => x.OldGroupId == oldGroupId && x.NewGroupId == newGroupId))
                )
                .ReturnsAsync(new List<Claim>());
            updatedResidentTeamGroupId.GroupId.Should().Be(newGroupId);
        }
    }
}
