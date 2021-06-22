using System;
using EvidenceApi.V1.Boundary.Request;
using System.Threading.Tasks;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IDocumentsApiGateway
    {
        Task<Claim> CreateClaim(ClaimRequest request);
        Task<Claim> UpdateClaim(Guid id, ClaimUpdateRequest request);
        Task<S3UploadPolicy> CreateUploadPolicy(Guid id);
        Task UploadDocument(Guid documentId, DocumentSubmissionRequest request);
        Task<Claim> GetClaimById(string id);
    }
}
