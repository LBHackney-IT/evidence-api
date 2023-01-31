using System;
using System.Collections.Generic;

namespace EvidenceApi.V1.Domain;

public class GroupResidentIdClaimIdBackfillObject
{
    public Guid? ResidentId { get; set; }
    public Guid? GroupId { get; set; }
    public List<string> ClaimIds { get; set; }
}
