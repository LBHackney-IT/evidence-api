using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentValidation;

namespace EvidenceApi.V1.UseCase
{
    public class ResidentRequestValidator : AbstractValidator<ResidentRequest>, IResidentRequestValidator
    {
        public ResidentRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Email).NotEmpty().When(x => string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("'Email' and 'Phone number' cannot be both empty.");
            RuleFor(x => x.PhoneNumber).NotEmpty().When(x => string.IsNullOrEmpty(x.Email)).WithMessage("'Email' and 'Phone number' cannot be both empty.");
        }
    }
}
