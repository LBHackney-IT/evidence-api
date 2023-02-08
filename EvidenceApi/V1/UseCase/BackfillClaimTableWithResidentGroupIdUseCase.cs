using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.Extensions.Logging;

namespace EvidenceApi.V1.UseCase;

public class BackfillClaimTableWithResidentGroupIdUseCase : IBackfillClaimTableWithResidentGroupIdUseCase
{
    private readonly IResidentsGateway _residentsGateway;
    private readonly IEvidenceGateway _evidenceGateway;
    private readonly IDocumentsApiGateway _documentsApiGateway;
    private readonly ILogger<BackfillClaimTableWithResidentGroupIdUseCase> _logger;

    public BackfillClaimTableWithResidentGroupIdUseCase(IResidentsGateway residentsGateway, IEvidenceGateway evidenceGateway, IDocumentsApiGateway documentsApiGateway, ILogger<BackfillClaimTableWithResidentGroupIdUseCase> logger)
    {
        _residentsGateway = residentsGateway;
        _evidenceGateway = evidenceGateway;
        _documentsApiGateway = documentsApiGateway;
        _logger = logger;
    }

    public async Task<List<ClaimBackfillResponse>> ExecuteAsync(string groupIdCharacters)
    {
        var initialObject = _residentsGateway.GetAllResidentIdsAndGroupIdsByFirstCharacter(groupIdCharacters);

        var filledObject = _evidenceGateway.GetClaimIdsForResidentsWithGroupIds(initialObject);

        var result = await _documentsApiGateway.BackfillClaimsWithGroupIds(filledObject);

        return result;
    }
}
