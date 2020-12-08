using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using FluentValidation.Results;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IEvidenceRequestValidator
    {
        ValidationResult Validate(EvidenceRequestRequest instance);
    }
}
