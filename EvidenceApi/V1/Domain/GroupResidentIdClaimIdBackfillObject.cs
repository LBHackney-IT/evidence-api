using System;

namespace EvidenceApi.V1.Domain;

public class GroupResidentIdClaimIdBackfillObject
{
    public Guid? ResidentId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? ClaimId { get; set; }
}
