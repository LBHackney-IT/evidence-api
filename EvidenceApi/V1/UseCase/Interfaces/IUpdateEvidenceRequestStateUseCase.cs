using System;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IUpdateEvidenceRequestStateUseCase
    {
        EvidenceRequest Execute(Guid id);
    }
}
