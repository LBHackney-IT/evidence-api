using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase;

public class GetAllGroupIdsByResidentIdsUseCase : IGetAllGroupIdsByResidentIdsUseCase
{
    private readonly IResidentsGateway _residentsGateway;
    private readonly ILogger<GetAllGroupIdsByResidentIdsUseCase> _logger;

    public GetAllGroupIdsByResidentIdsUseCase(IResidentsGateway residentsGateway, ILogger<GetAllGroupIdsByResidentIdsUseCase> logger)
    {
        _residentsGateway = residentsGateway;
        _logger = logger;
    }

    public List<GroupResidentIdClaimIdBackfillObject> Execute()
    {
        return _residentsGateway.GetAllResidentIdsAndGroupIds();
    }
}
