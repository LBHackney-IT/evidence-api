using System.Collections.Generic;
using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class FindOrCreateResidentReferenceIdUseCase : IFindOrCreateResidentReferenceIdUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IStringHasher _stringHasher;
        private const int ResidentReferenceLength = 11;

        public FindOrCreateResidentReferenceIdUseCase(IEvidenceGateway evidenceGateway, IStringHasher stringHasher)
        {
            _evidenceGateway = evidenceGateway;
            _stringHasher = stringHasher;
        }

        public string Execute(Resident resident)
        {
            var residentId = resident.Id;
            var evidenceRequestsForResident = _evidenceGateway.FindEvidenceRequestsByResidentId(residentId);
            if (evidenceRequestsForResident.Count > 0)
            {
                return evidenceRequestsForResident.First().ResidentReferenceId;
            }

            var residentIdHash = _stringHasher.Create(residentId.ToString());
            var hashStartIndex = 0;
            var residentReferenceId = TrimHash(residentIdHash, hashStartIndex, ResidentReferenceLength);
            var allResidentReferenceIds = _evidenceGateway.GetAll().Select(e => e.ResidentReferenceId).ToHashSet();
            var residentReferenceIdAlreadyExists = ResidentReferenceIdExists(allResidentReferenceIds, residentReferenceId);
            while (residentReferenceIdAlreadyExists)
            {
                hashStartIndex++;
                residentReferenceId = TrimHash(residentIdHash, hashStartIndex, ResidentReferenceLength);
                residentReferenceIdAlreadyExists = ResidentReferenceIdExists(allResidentReferenceIds, residentReferenceId);
            }

            return residentReferenceId;
        }

        private static string TrimHash(string hash, int startIndex, int length)
        {
            return hash.Substring(startIndex, length).Replace("-", "");
        }

        private static bool ResidentReferenceIdExists(ICollection<string> residentReferenceIds, string newResidentReferenceId)
        {
            return residentReferenceIds.Contains(newResidentReferenceId);
        }
    }
}
