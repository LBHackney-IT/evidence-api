using System;
using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IDocumentsApiGateway
    {
        Task<Claim> CreateClaim(ClaimRequest request);
        Task<Claim> UpdateClaim(Guid id, ClaimUpdateRequest request);
        Task<Claim> GetClaimById(string id);
        Task<List<Claim>> GetClaimsByIdsThrottled(List<string> claimIds);
        Task<S3UploadPolicy> CreateUploadPolicy(Guid id);
        Task<List<ClaimBackfillResponse>> BackfillClaimsWithGroupIds(List<GroupResidentIdClaimIdBackfillObject> backfillObjects);
        Task<PaginatedClaimResponse> GetClaimsByGroupId(PaginatedClaimRequest request);
    }
}
