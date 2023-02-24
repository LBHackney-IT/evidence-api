using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase
{
    public class AmendResidentGroupIdUseCase : IAmendResidentGroupIdUseCase
    {
        private readonly IResidentsGateway _residentsGateway;
        private readonly IDocumentsApiGateway _documentsApiGateway;
        private readonly ILogger<AmendResidentGroupIdUseCase> _logger;

        public AmendResidentGroupIdUseCase(IResidentsGateway residentsGateway, IDocumentsApiGateway documentsApiGateway, ILogger<AmendResidentGroupIdUseCase> logger)
        {
            _residentsGateway = residentsGateway;
            _documentsApiGateway = documentsApiGateway;
            _logger = logger;
        }

        public async Task<ResidentsTeamGroupId> Execute(ResidentGroupIdRequest request)
        {
            if (request.Team == null)
            {
                throw new BadRequestException("Team must not be null");
            }
            var residentTeamGroupId = _residentsGateway.FindResidentTeamGroupIdByResidentIdAndTeam(request.ResidentId, request.Team);
            if (residentTeamGroupId == null)
            {
                throw new NotFoundException("No record found for that residentId and team");
            }
            var oldGroupId = residentTeamGroupId.GroupId;
            var updatedResidentTeamGroupId = _residentsGateway.UpdateResidentGroupId(request.ResidentId, request.Team, request.GroupId);
            try
            {
                var claimsUpdateRequest = new ClaimsUpdateRequest() { OldGroupId = oldGroupId, NewGroupId = request.GroupId };
                var claims = await _documentsApiGateway.UpdateClaimsGroupId(claimsUpdateRequest);
            }
            catch (DocumentsApiException ex)
            {
                throw new BadRequestException($"Issue with DocumentsApi so cannot update claims: {ex.Message}");
            }
            return updatedResidentTeamGroupId;
        }
    }
}
