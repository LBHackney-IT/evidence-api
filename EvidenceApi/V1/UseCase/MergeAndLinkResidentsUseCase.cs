using System;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Factories;

namespace EvidenceApi.V1.UseCase
{
    public class MergeAndLinkResidentsUseCase : IMergeAndLinkResidentsUseCase
    {
        public ICreateMergedResidentUseCase _createMergedResidentUseCase;
        public IResidentsGateway _residentsGateway;
        public IEvidenceGateway _evidenceGateway;
        public IAmendClaimsGroupIdUseCase _amendClaimsGroupIdUseCase;

        public MergeAndLinkResidentsUseCase(
            ICreateMergedResidentUseCase CreateMergedResidentUseCase,
            IResidentsGateway residentGateway,
            IEvidenceGateway evidenceGateway,
            IAmendClaimsGroupIdUseCase amendResidentGroupIdUseCase)
        {
            _createMergedResidentUseCase = CreateMergedResidentUseCase;
            _residentsGateway = residentGateway;
            _evidenceGateway = evidenceGateway;
            _amendClaimsGroupIdUseCase = amendResidentGroupIdUseCase;
        }
        public async Task<MergeAndLinkResidentsResponse> ExecuteAsync(MergeAndLinkResidentsRequest request)
        {
            var residentRequest = new ResidentRequest()
            {
                Name = request.NewResident.Name,
                Email = request.NewResident.Email,
                PhoneNumber = request.NewResident.PhoneNumber,
                Team = request.Team,
                GroupId = request.GroupId
            };
            var resident = _createMergedResidentUseCase.Execute(residentRequest);
            _evidenceGateway.UpdateResidentIdForDocumentSubmission(resident.Id, request.ResidentsToDelete);

            foreach (Guid residentId in request.ResidentsToDelete)
            {
                var residentGroupIdRequest = new ResidentGroupIdRequest()
                {
                    ResidentId = residentId,
                    Team = request.Team,
                    GroupId = request.GroupId
                };
                await _amendClaimsGroupIdUseCase.Execute(residentGroupIdRequest);

                var residentTeamGroupIds = _residentsGateway.FindResidentTeamGroupIdsByResidentId(residentId);

                foreach (ResidentsTeamGroupId residentsTeamGroupId in residentTeamGroupIds)
                {
                    if (residentsTeamGroupId.Team != request.Team)
                    {
                        _residentsGateway.AddResidentGroupId(resident.Id, residentsTeamGroupId.Team, residentsTeamGroupId.GroupId);
                    }
                }
                _residentsGateway.HideResident(residentId);
            }
            return resident.ToResponse(request.GroupId);
        }
    }
}
