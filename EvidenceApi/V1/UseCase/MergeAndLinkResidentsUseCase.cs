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
        // 1. delete residents by residentId provided in the request
        // 2. create new resident based on info in the request - done
        // 3. add entry groupId in ResidentsTeamGroupId, GroupId in claims table (doc api), add residentId in document_submissions table for each deleted resident - done
        // 4. add entry in ResidentsTeamGroupId table for each pair of (resident, team) apart of the team that sent the request - done

        public ICreateMergedResidentUseCase _createMergedResidentUseCase;
        public IResidentsGateway _residentsGateway;
        public IEvidenceGateway _evidenceGateway;
        public IAmendClaimsGroupIdUseCase _amendResidentGroupIdUseCase;

        public MergeAndLinkResidentsUseCase(
            ICreateMergedResidentUseCase CreateMergedResidentUseCase,
            IResidentsGateway residentGateway,
            IEvidenceGateway evidenceGateway,
            IAmendClaimsGroupIdUseCase amendResidentGroupIdUseCase)
        {
            _createMergedResidentUseCase = CreateMergedResidentUseCase;
            _residentsGateway = residentGateway;
            _evidenceGateway = evidenceGateway;
            _amendResidentGroupIdUseCase = amendResidentGroupIdUseCase;
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
                await _amendResidentGroupIdUseCase.Execute(residentGroupIdRequest);

                var residentTeamGroupIds = _residentsGateway.FindResidentTeamGroupIdsByResidentId(residentId);
                foreach (ResidentsTeamGroupId residentsTeamGroupId in residentTeamGroupIds)
                {
                    if (residentsTeamGroupId.Team != request.Team)
                    {
                        _residentsGateway.AddResidentGroupId(resident.Id, request.Team, request.GroupId);
                    }
                }
                _residentsGateway.HideResident(residentId);
            }
            return resident.ToResponse(request.GroupId);
        }
    }
}
