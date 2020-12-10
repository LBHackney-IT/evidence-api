using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentValidation;

namespace EvidenceApi.V1.UseCase
{
    public class ResidentRequestValidator : AbstractValidator<ResidentRequest>, IResidentRequestValidator
    {
        public ResidentRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty();
        }
    }
}
