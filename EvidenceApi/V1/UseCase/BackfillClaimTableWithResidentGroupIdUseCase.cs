using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase;

public class BackfillClaimTableWithResidentGroupIdUseCase : IBackfillClaimTableWithResidentGroupIdUseCase
{
    private readonly IResidentsGateway _residentsGateway;
    private readonly IEvidenceGateway _evidenceGateway;
    private readonly ILogger<BackfillClaimTableWithResidentGroupIdUseCase> _logger;

    public BackfillClaimTableWithResidentGroupIdUseCase(IResidentsGateway residentsGateway, IEvidenceGateway evidenceGateway, ILogger<BackfillClaimTableWithResidentGroupIdUseCase> logger)
    {
        _residentsGateway = residentsGateway;
        _evidenceGateway = evidenceGateway;
        _logger = logger;
    }

    public List<GroupResidentIdClaimIdBackfillObject> Execute()
    {
        var initialObject = _residentsGateway.GetAllResidentIdsAndGroupIds();
        return _evidenceGateway.GetClaimIdsForResidentsWithGroupIds(initialObject);
        //one more step - insert into claims table using patch endpoint
    }
}
