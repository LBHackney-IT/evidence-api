using FluentValidation;
using EvidenceApi.V1.Boundary.Request;

namespace EvidenceApi.V1.Validators
{
    public class ResidentRequestValidator : AbstractValidator<ResidentRequest>
    {
        public ResidentRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().When(x => string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("'Email' and 'Phone number' cannot be both empty.");
            RuleFor(x => x.PhoneNumber).NotEmpty().When(x => string.IsNullOrEmpty(x.Email)).WithMessage("'Email' and 'Phone number' cannot be both empty.");
        }
    }
}
