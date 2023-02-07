using System;

namespace EvidenceApi.V1.Boundary.Response;

public class ClaimBackfillResponse
{
    public Guid ClaimId { get; set; }
    public Guid? GroupId { get; set; }
}
