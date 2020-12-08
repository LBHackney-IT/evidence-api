using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentValidation;

namespace EvidenceApi.V1.UseCase
{
    public class EvidenceRequestValidator : AbstractValidator<EvidenceRequestRequest>, IEvidenceRequestValidator
    {
        public EvidenceRequestValidator(IValidator<ResidentRequest> residentValidator)
        {
            RuleFor(x => x.ServiceRequestedBy).NotEmpty();

            RuleFor(x => x.DocumentType).NotEmpty();

            RuleFor(x => x.DeliveryMethods).NotNull();
            RuleForEach(x => x.DeliveryMethods)
                .IsEnumName(typeof(EvidenceRequest.DeliveryMethod), false);

            RuleFor(x => x.Resident)
                .NotEmpty()
                .SetValidator(residentValidator);
        }
    }
}
