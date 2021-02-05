using System;
using EvidenceApi.V1.Domain.Enums;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IUpdateEvidenceRequestStateUseCase
    {
        EvidenceRequestState Execute(Guid id);
    }
}
